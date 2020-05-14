using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public class Image
    {
        public int ImageID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public ICollection<Container> Containers { get; set; }
        public string Tag { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastModifiedTime { get; set; }
    }
}