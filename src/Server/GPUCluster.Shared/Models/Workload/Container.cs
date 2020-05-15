using System;
using System.ComponentModel.DataAnnotations;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public class Container
    {
        public int ContainerID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public int ImageID { get; set; }
        public Image Image { get; set; }
        public string Name { get; set; }
        public bool IsRunning { get; set; }
    }
}