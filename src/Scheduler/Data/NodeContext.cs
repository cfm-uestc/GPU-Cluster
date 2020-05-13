using Microsoft.EntityFrameworkCore;

namespace Scheduler.Data
{
    public class NodeContext : DbContext
    {
        public NodeContext (
            DbContextOptions<NodeContext> options)
            : base(options)
        {
        }

        public DbSet<Scheduler.Models.Node> Node { get; set; }
    }
}