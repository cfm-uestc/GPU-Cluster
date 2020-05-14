using System;
using System.Linq;
using GPUCluster.Shared.Models.Instance;
using GPUCluster.Shared.Models.Workload;

namespace GPUCluster.WebService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ManagerContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var groups = new Group[]
            {
                new Group{Role="normal"},
                new Group{Role="admin"},
            };
            foreach (Group c in groups)
            {
                context.Groups.Add(c);
            }
            context.SaveChanges();

            var users = new User[]
            {
                new User{Name="zhuxiaosu1",GroupID=1},
                new User{Name="zhuxiaosu2",GroupID=2},
                new User{Name="zhuxiaosu3",GroupID=1}
            };
            foreach (User s in users)
            {
                context.Users.Add(s);
            }
            context.SaveChanges();

            var images = new Image[]
            {
                new Image{Tag="zhuxiaosu1_base",UserID=1,CreateTime=DateTime.Today,LastModifiedTime=DateTime.Now},
                new Image{Tag="zhuxiaosu2_base",UserID=2,CreateTime=DateTime.Today.AddDays(-1),LastModifiedTime=DateTime.Now.AddDays(-1)},
                new Image{Tag="zhuxiaosu3_base",UserID=3,CreateTime=DateTime.Today.AddDays(-2),LastModifiedTime=DateTime.Now.AddDays(-1)}
            };
            foreach (Image i in images)
            {
                context.Images.Add(i);
            }
            context.SaveChanges();

            var containers = new Container[]
            {
                new Container{UserID=1,ImageID=1,Name="zxs1_container",IsRunning=true},
            };
            foreach (Container n in containers)
            {
                context.Containers.Add(n);
            }
            context.SaveChanges();
        }
    }
}