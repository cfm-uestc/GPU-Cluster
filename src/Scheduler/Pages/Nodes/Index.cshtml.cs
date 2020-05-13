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
    public class IndexModel : PageModel
    {
        private readonly Scheduler.Data.NodeContext _context;

        public IndexModel(Scheduler.Data.NodeContext context)
        {
            _context = context;
        }

        public IList<Node> Node { get;set; }

        public async Task OnGetAsync()
        {
            Node = await _context.Node.ToListAsync();
        }
    }
}
