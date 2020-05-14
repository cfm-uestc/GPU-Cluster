using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Workload;

namespace GPUCluster.Shared.Models.Instance
{
    public class User
    {
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public string Name { get; set; }
        public ICollection<Container> Enrollments { get; set; }
    }
}