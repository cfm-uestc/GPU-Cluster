using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Scheduler.Data;
using Scheduler.Models;

namespace Scheduler.Pages.Nodes
{
    public class DeleteModel : PageModel
    {
        private readonly Scheduler.Data.NodeContext _context;

        public DeleteModel(Scheduler.Data.NodeContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Node Node { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Node = await _context.Node.FirstOrDefaultAsync(m => m.ID == id);

            if (Node == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Node = await _context.Node.FindAsync(id);

            if (Node != null)
            {
                _context.Node.Remove(Node);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
