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

namespace GPUCluster.WebService.Controllers
{
    public class ContainersController : Controller
    {
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IdentityDataContext _context;

        public ContainersController(IdentityDataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _manager = userManager;
        }

        // GET: Containers
        public async Task<IActionResult> Index()
        {
            var identityDataContext = _context.Container.Include(c => c.Image).Include(c => c.User);
            return View(await identityDataContext.ToListAsync());
        }

        // GET: Containers/Details/5
        public async Task<IActionResult> Details(int? id)
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
            ViewData["ImageID"] = new SelectList(_context.Set<Image>(), "ImageID", "ImageID");
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Containers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContainerID,UserID,ImageID,Name,IsRunning")] Container container)
        {
            if (ModelState.IsValid)
            {
                _context.Add(container);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ImageID"] = new SelectList(_context.Set<Image>(), "ImageID", "ImageID", container.ImageID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", container.UserID);
            return View(container);
        }

        // GET: Containers/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["ImageID"] = new SelectList(_context.Set<Image>(), "ImageID", "ImageID", container.ImageID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", container.UserID);
            return View(container);
        }

        // POST: Containers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContainerID,UserID,ImageID,Name,IsRunning")] Container container)
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
            ViewData["ImageID"] = new SelectList(_context.Set<Image>(), "ImageID", "ImageID", container.ImageID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", container.UserID);
            return View(container);
        }

        // GET: Containers/Delete/5
        public async Task<IActionResult> Delete(int? id)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var container = await _context.Container.FindAsync(id);
            _context.Container.Remove(container);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContainerExists(int id)
        {
            return _context.Container.Any(e => e.ContainerID == id);
        }
    }
}
