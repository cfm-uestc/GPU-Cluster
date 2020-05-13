using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Scheduler.Data;
using Scheduler.Models;

namespace Scheduler.Pages.Nodes
{
    public class CreateModel : PageModel
    {
        private readonly Scheduler.Data.NodeContext _context;

        public CreateModel(Scheduler.Data.NodeContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Node Node { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Node.Add(Node);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
