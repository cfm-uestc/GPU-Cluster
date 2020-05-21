using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Workload;
using Microsoft.AspNetCore.Identity;

namespace GPUCluster.Shared.Models.Instance
{
    public class ApplicationUser : IdentityUser
    {
        public int LinuxUserID { get; set; }
        public LinuxUser LinuxUser { get; set; }
        public ICollection<Container> Containers { get; set; }
        public ICollection<Image> Images { get; set; }
        public ICollection<Mounting> Mountings { get; set; }
    }
}