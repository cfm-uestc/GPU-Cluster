using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GPUCluster.Shared.Models.Workload;
using Microsoft.AspNetCore.Identity;

namespace GPUCluster.Shared.Models.Instance
{
    public class ApplicationUser: IdentityUser
    {
        public ICollection<Container> Containers { get; set; }
    }
}