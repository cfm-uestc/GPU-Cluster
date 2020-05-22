using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPUCluster.Shared.Models.Instance;
using GPUCluster.Shared.Models.Workload;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GPUCluster.WebService.Areas.Identity.Data
{
    public class IdentityDataContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IUserProvider _userProvider;
        public IdentityDataContext(DbContextOptions<IdentityDataContext> options, IUserProvider userProvider)
            : base(options)
        {
            _userProvider = userProvider;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<GPUCluster.Shared.Models.Workload.Container>().HasQueryFilter(f => f.UserID == _userProvider.GetUserId());
            builder.Entity<GPUCluster.Shared.Models.Workload.Volume>().HasQueryFilter(f => f.UserID == _userProvider.GetUserId());
            builder.Entity<GPUCluster.Shared.Models.Workload.Mounting>().HasQueryFilter(f => f.UserID == _userProvider.GetUserId());
            builder.Entity<GPUCluster.Shared.Models.Workload.Image>().HasQueryFilter(f => f.UserID == _userProvider.GetUserId()).HasIndex(f => f.Tag).IsUnique();
        }

        public DbSet<GPUCluster.Shared.Models.Instance.LinuxUser> LinuxUser { get; set; }
        public DbSet<GPUCluster.Shared.Models.Workload.Container> Container { get; set; }

        public DbSet<GPUCluster.Shared.Models.Workload.Image> Image { get; set; }
        public DbSet<GPUCluster.Shared.Models.Workload.Volume> Volume { get; set; }
        public DbSet<GPUCluster.Shared.Models.Workload.Mounting> Mounting { get; set; }
    }
}
