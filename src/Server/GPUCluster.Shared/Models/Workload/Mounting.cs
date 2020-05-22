using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public class Mounting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public Guid ContainerID { get; set; }
        public Container Container { get; set; }
        public Guid VolumeID { get; set; }
        public Volume Volume { get; set; }
    }
}