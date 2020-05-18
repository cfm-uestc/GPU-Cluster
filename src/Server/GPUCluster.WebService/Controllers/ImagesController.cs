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

namespace GPUCluster.WebService.Controllers
{
    public class ImagesController : Controller
    {
        private readonly IdentityDataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ImagesController(IdentityDataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Images
        public async Task<IActionResult> Index()
        {
            var identityDataContext = _context.Image.Include(i => i.User);
            return View(await identityDataContext.ToListAsync());
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

        public IActionResult CreateFinish()
        {
            return RedirectToAction(nameof(Index));
        }

        // POST: Images/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImageID,UserID,Tag,CreateTime,LastModifiedTime")] Image image)
        {
            ViewData["CurrentUser"] = await _userManager.GetUserAsync(this.User);
            if (ModelState.IsValid)
            {
                image.User = await _userManager.GetUserAsync(this.User);
                if (!image.Tag.StartsWith(image.User.UserName + "_"))
                {
                    image.Tag = $"{image.User.UserName}_{image.Tag}";
                }
                if ((await _context.Image.FirstOrDefaultAsync(f => f.Tag == image.Tag)) != null)
                {
                    ModelState.AddModelError("Tag", "Image Tag already exists.");
                    return PartialView("Partial/_CreateForm", image);
                }
                image.CreateTime = DateTime.Now;
                image.LastModifiedTime = image.CreateTime;
                ViewBag.Creating = true;
                ViewData["CreateResult"] = "Preparing Dockerfile to build..." + Environment.NewLine;
                Stream result = await image.CreateAndBuildAsync();
                using (var reader = new StreamReader(result))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        ViewData["CreateResult"] += line;
                    }
                }
                _context.Add(image);
                await _context.SaveChangesAsync();
                ViewBag.Ready = true;
                return PartialView("Partial/_CreateIndicator");
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
