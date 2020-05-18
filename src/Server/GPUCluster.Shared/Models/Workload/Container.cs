using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public class Container
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ContainerID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public int ImageID { get; set; }
        public Image Image { get; set; }
        public string Name { get; set; }
        public bool IsRunning { get; set; }
    }
}