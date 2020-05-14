using Microsoft.EntityFrameworkCore;
using GPUCluster.Shared.Models.Workload;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.WebService.Data
{
    public class ManagerContext : DbContext
    {
        public ManagerContext(DbContextOptions<ManagerContext> options)
            : base(options)
        {
        }

        public DbSet<Container> Containers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}