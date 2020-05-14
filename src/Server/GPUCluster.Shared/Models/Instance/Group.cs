using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Workload;

namespace GPUCluster.Shared.Models.Instance
{
    public class Group
    {
        public int GroupID { get; set; }
        public string Role { get; set; }
        public ICollection<User> Users { get; set; }
    }
}