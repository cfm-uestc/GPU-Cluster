using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GPUCluster.Shared.Models.Workload;
using GPUCluster.WebService.Areas.Identity.Data;

namespace GPUCluster.WebService.Controllers
{
    public class MountingsController : Controller
    {
        private readonly IdentityDataContext _context;

        public MountingsController(IdentityDataContext context)
        {
            _context = context;
        }

        // GET: Mountings
        public async Task<IActionResult> Index()
        {
            var identityDataContext = _context.Mounting.Include(m => m.Container).Include(m => m.User);
            return View(await identityDataContext.ToListAsync());
        }

        // GET: Mountings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mounting = await _context.Mounting
                .Include(m => m.Container)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MountingID == id);
            if (mounting == null)
            {
                return NotFound();
            }

            return View(mounting);
        }

        // GET: Mountings/Create
        public IActionResult Create()
        {
            ViewData["ContainerID"] = new SelectList(_context.Container, "ContainerID", "ContainerID");
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Mountings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MountingID,UserID,ContainerID,Type,Path")] Mounting mounting)
        {
            if (ModelState.IsValid)
            {
                mounting.MountingID = Guid.NewGuid();
                _context.Add(mounting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContainerID"] = new SelectList(_context.Container, "ContainerID", "ContainerID", mounting.ContainerID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", mounting.UserID);
            return View(mounting);
        }

        // GET: Mountings/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mounting = await _context.Mounting.FindAsync(id);
            if (mounting == null)
            {
                return NotFound();
            }
            ViewData["ContainerID"] = new SelectList(_context.Container, "ContainerID", "ContainerID", mounting.ContainerID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", mounting.UserID);
            return View(mounting);
        }

        // POST: Mountings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("MountingID,UserID,ContainerID,Type,Path")] Mounting mounting)
        {
            if (id != mounting.MountingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mounting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MountingExists(mounting.MountingID))
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
            ViewData["ContainerID"] = new SelectList(_context.Container, "ContainerID", "ContainerID", mounting.ContainerID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", mounting.UserID);
            return View(mounting);
        }

        // GET: Mountings/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mounting = await _context.Mounting
                .Include(m => m.Container)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MountingID == id);
            if (mounting == null)
            {
                return NotFound();
            }

            return View(mounting);
        }

        // POST: Mountings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var mounting = await _context.Mounting.FindAsync(id);
            _context.Mounting.Remove(mounting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MountingExists(Guid id)
        {
            return _context.Mounting.Any(e => e.MountingID == id);
        }
    }
}
