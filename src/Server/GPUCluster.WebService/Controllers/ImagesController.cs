using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GPUCluster.Shared.Models.Workload;
using GPUCluster.WebService.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using GPUCluster.Shared.Models.Instance;
using System.IO;
using GPUCluster.WebService.Service;
using Lib.AspNetCore.ServerSentEvents;
using GPUCluster.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;
using GPUCluster.Shared.Events;
using Docker.DotNet.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace GPUCluster.WebService.Controllers
{
    [Authorize]
    public class ImagesController : Controller
    {
        private readonly IdentityDataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageCreationSSEService _imageCreationSSEService;
        public ImagesController(IdentityDataContext context, UserManager<ApplicationUser> userManager, IImageCreationSSEService imageCreationSSEService)
        {
            _context = context;
            _userManager = userManager;
            _imageCreationSSEService = imageCreationSSEService;
        }

        // GET: Images
        public async Task<IActionResult> Index()
        {
            var identityDataContext = _context.Image.Include(i => i.User);
            return View(await identityDataContext.ToListAsync());
        }
        // GET: RedirectIndex
        public IActionResult RedirectIndex()
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: Images/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Image
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.VolumeID == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // GET: Images/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CurrentUser"] = await _userManager.GetUserAsync(this.User);
            return View();
        }

        private async Task pushDockerBuildStream(IServerSentEventsClient client, Stream buildOutput)
        {
            using (var reader = new StreamReader(buildOutput))
            {
                while (!reader.EndOfStream)
                {
                    var result = await reader.ReadLineAsync();
                    if (result.Contains("Success"))
                    {

                    }
                    await client.SendEventAsync(new ServerSentEvent()
                    {
                        Type = "BuildOutput",
                        Data = new string[] { result }
                    });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckImageCreated([Bind("BaseImageTag,VolumeID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            var user = await _userManager.GetUserAsync(this.User);
            try
            {
                image = await validateImage(user, image);
            }
            catch (ArgumentException)
            {
                return new JsonResult(new BasicResponse()
                {
                    Status = "ok",
                    Message = "Image already created.",
                    Code = 200
                });
            }
            return new JsonResult(new BasicResponse()
            {
                Status = "not-found",
                Message = "Image not found.",
                Code = 404
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TryDiscardChange([Bind("BaseImageTag,VolumeID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            try
            {
                var user = await _userManager.GetUserAsync(this.User);
                image = await validateImage(user, image);
                return Ok();
            }
            catch (ArgumentException)
            {
                image = await _context.Image.FirstOrDefaultAsync(f => f.Tag == image.Tag);
                _context.Remove(image);
                await _context.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuildAndCreate([Bind("BaseImageTag,VolumeID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            var user = await _userManager.GetUserAsync(this.User);
            user.LinuxUser = await _context.LinuxUser.FirstAsync(u => u.ID == user.LinuxUserID);
            image = await validateImage(user, image);
            var clients = _imageCreationSSEService.GetClients();
            var currentClient = clients.FirstOrDefault(x => _userManager.GetUserId(x.User) == user.Id);
            EventHandler<JSONMessage> handler = async (s, e) =>
            {
                if (currentClient == null)
                {
                    currentClient = clients.FirstOrDefault(x => _userManager.GetUserId(x.User) == user.Id);
                }
                try
                {
                    if (currentClient != null && currentClient.IsConnected)
                        await currentClient?.SendEventAsync(new ServerSentEvent()
                        {
                            Type = "BuildOutput",
                            Data = new string[] { JsonConvert.SerializeObject(e, new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            }) }
                        });
                }
                catch (InvalidOperationException)
                {
                    // Occurs when client offline
                    // TODO: When offline a while, cancel the task
                    currentClient = null;
                }
            };

            image.BuildStatusChanged += handler;
            try
            {
                bool buildResult = await image.CreateAndBuildAsync();
                if (buildResult)
                {
                    currentClient = currentClient ?? clients.FirstOrDefault(x => _userManager.GetUserId(x.User) == user.Id);
                    await currentClient?.SendEventAsync(new ServerSentEvent()
                    {
                        Type = "BuildFinished",
                        Data = new string[] { "ok" }
                    });
                    bool pushResult = await image.PushDockerImageAsync();
                    await currentClient?.SendEventAsync(new ServerSentEvent()
                    {
                        Type = "PushFinished",
                        Data = new string[] { "ok" }
                    });
                    _context.Add(image);
                    await _context.SaveChangesAsync();
                    image.BuildStatusChanged -= handler;
                    await currentClient?.SendEventAsync(new ServerSentEvent()
                    {
                        Type = "Success",
                        Data = new string[] { "ok" }
                    });
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (System.Exception)
            {
                return BadRequest();
            }
        }


        private async Task<Image> validateImage(ApplicationUser user, Image image)
        {
            image.User = user;
            image.UserID = user.Id;
            if (!image.Tag.StartsWith(image.User.UserName + "_"))
            {
                image.Tag = $"{image.User.UserName}_{image.Tag}";
            }
            if ((await _context.Image.FirstOrDefaultAsync(f => f.Tag == image.Tag)) != null)
            {
                throw new ArgumentException();
            }
            if (string.IsNullOrWhiteSpace(image.BaseImageTag))
            {
                image.BaseImageTag = Image.DefaultBaseImage;
            }
            var volume = new Volume()
            {
                Type = VolumeType.ReadWrite,
                Path = VolumePath.Home,
                Image = image,
                User = user,
                UserID = user.Id
            };
            image.Volume = volume;
            image.CreateTime = DateTime.Now;
            image.LastModifiedTime = image.CreateTime;
            return image;
        }

        // POST: Images/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BaseImageTag,VolumeID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            ApplicationUser user = await _userManager.GetUserAsync(this.User);
            ViewData["CurrentUser"] = user;
            if (ModelState.IsValid)
            {
                try
                {
                    image = await validateImage(user, image);
                    ViewBag.Creating = true;
                    return PartialView("Partial/_CreateForm", image);
                }
                catch (ArgumentException)
                {
                    ModelState.AddModelError("Tag", "Image Tag already exists.");
                    return PartialView("Partial/_CreateForm", image);
                }
            }
            return View(image);
        }

        // GET: Images/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Image.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", image.UserID);
            return View(image);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("VolumeID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            if (id != image.VolumeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(image);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImageExists(image.VolumeID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", image.UserID);
            return View(image);
        }

        // GET: Images/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Image
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.VolumeID == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var image = await _context.Image.FindAsync(id);
            _context.Image.Remove(image);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImageExists(Guid id)
        {
            return _context.Image.Any(e => e.VolumeID == id);
        }
    }
}
