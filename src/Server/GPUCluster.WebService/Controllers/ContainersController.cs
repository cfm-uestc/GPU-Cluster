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
using Microsoft.AspNetCore.Authorization;
namespace GPUCluster.WebService.Controllers
{
    [Authorize]
    public class ContainersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityDataContext _context;

        public ContainersController(IdentityDataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Containers
        public async Task<IActionResult> Index()
        {
            var identityDataContext = _context.Container.Include(c => c.Image).Include(c => c.User);
            return View(await identityDataContext.ToListAsync());
        }

        // GET: Containers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Container
                .Include(c => c.Image)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.ContainerID == id);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }

        // GET: Containers/Create
        public IActionResult Create()
        {
            ViewData["ImageID"] = new SelectList(_context.Set<Image>(), "ImageID", "Tag");
            return View();
        }

        public Container validateContainer(ApplicationUser user, Container container)
        {
            container.User = user;
            container.UserID = user.Id;
            var finded = _context.Container.Where(x => x.Name == container.Name && x.UserID == user.Id).ToList();
            if (finded.Count > 0)
            {
                throw new ArgumentException();
            }
            return container;
        }

        // POST: Containers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContainerID,UserID,ImageID,Name,IsRunning")] Container container)
        {
            ApplicationUser user = await _userManager.GetUserAsync(this.User);
            if (ModelState.IsValid)
            {
                try
                {
                    container = validateContainer(user, container);
                    _context.Add(container);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    ModelState.AddModelError("Name", "Container name already exists.");
                    return View(container);
                }
            }
            ViewData["ImageID"] = new SelectList(_context.Image, "ImageID", "Tag", container.ImageID);
            return View(container);
        }

        // GET: Containers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Container.FindAsync(id);
            if (container == null)
            {
                return NotFound();
            }
            ViewData["ImageID"] = new SelectList(_context.Image, "ImageID", "ImageID", container.ImageID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", container.UserID);
            return View(container);
        }

        // POST: Containers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ContainerID,UserID,ImageID,Name,IsRunning")] Container container)
        {
            if (id != container.ContainerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(container);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerExists(container.ContainerID))
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
            ViewData["ImageID"] = new SelectList(_context.Image, "ImageID", "ImageID", container.ImageID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", container.UserID);
            return View(container);
        }

        // GET: Containers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Container
                .Include(c => c.Image)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.ContainerID == id);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }

        // POST: Containers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var container = await _context.Container.FindAsync(id);
            _context.Container.Remove(container);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContainerExists(Guid id)
        {
            return _context.Container.Any(e => e.ContainerID == id);
        }
    }
}
