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

namespace GPUCluster.WebService.Controllers
{
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
                .FirstOrDefaultAsync(m => m.ImageID == id);
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
        public async Task<IActionResult> CheckImageCreated([Bind("BaseImageTag, ImageID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
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
        public async Task<IActionResult> CreateFinish([Bind("BaseImageTag, ImageID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            var user = await _userManager.GetUserAsync(this.User);
            image = await validateImage(user, image);
            var clients = _imageCreationSSEService.GetClients();
            var currentClient = clients.FirstOrDefault(x => _userManager.GetUserId(x.User) == user.Id);
            EventHandler<StatusChangedEventArgs> handler = async (s, e) =>
            {
                await currentClient.SendEventAsync(new ServerSentEvent()
                {
                    Type = "BuildOutput",
                    Data = new string[] { $"{{'msg':'{e.Message}'}}" }
                });
            };
            if (currentClient != null)
            {
                await currentClient.SendEventAsync(new ServerSentEvent()
                {
                    Type = "BuildOutput",
                    Data = new string[] { "{\"msg\":\"Prepare Dockerfile to build...\"}" }
                });
                image.BuildStatusChanged += handler;

            }
            using (Stream result = await image.CreateAndBuildAsync())
            {
                if (currentClient != null)
                {
                    await pushDockerBuildStream(currentClient, result);
                }
                else
                {

                }
            }
            _context.Add(image);
            await _context.SaveChangesAsync();
            image.BuildStatusChanged -= handler;
            await currentClient.SendEventAsync(new ServerSentEvent()
            {
                Type = "BuildFinished",
                Data = new string[] { "ok" }
            });
            return Ok();
        }


        private async Task<Image> validateImage(ApplicationUser user, Image image)
        {
            image.User = user;
            if (!image.Tag.StartsWith(image.User.UserName + "_"))
            {
                image.Tag = $"{image.User.UserName}_{image.Tag}";
            }
            if ((await _context.Image.FirstOrDefaultAsync(f => f.Tag == image.Tag)) != null)
            {
                throw new ArgumentException();
            }
            image.CreateTime = DateTime.Now;
            image.LastModifiedTime = image.CreateTime;
            return image;
        }

        // POST: Images/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BaseImageTag,ImageID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
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
        public async Task<IActionResult> Edit(Guid id, [Bind("ImageID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            if (id != image.ImageID)
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
                    if (!ImageExists(image.ImageID))
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
                .FirstOrDefaultAsync(m => m.ImageID == id);
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
            return _context.Image.Any(e => e.ImageID == id);
        }
    }
}