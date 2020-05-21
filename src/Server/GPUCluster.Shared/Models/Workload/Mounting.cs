using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public enum MountingType
    {
        Read,
        ReadWrite
    }
    public enum MountingPath
    {
        Home,
        Data,
        Public
    }
    public class Mounting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MountingID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public Guid ContainerID { get; set; }
        public Container Container { get; set; }
        public string Name { get; set; }
        public MountingType Type { get; set; }
        public MountingPath Path { get; set; }
    }
}