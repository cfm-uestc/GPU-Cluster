using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GPUCluster.Shared.Models.Workload;
using Microsoft.AspNetCore.Identity;

namespace GPUCluster.Shared.Models.Instance
{
    public class LinuxUser
    {
        public int ID { get; set; }
        public int ActualUID { get => ID + 1000; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IEnumerable<int> ExposePortRange
        {
            get => Enumerable.Range(20000 + ID * 25, 20025 + ID * 25);
        }
    }
}