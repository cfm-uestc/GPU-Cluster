using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scheduler.Models
{
    public class PodSpec
    {
        public string Image { get; set; }
    }
    public class DeploySpec
    {
        public PodSpec Pod { get; set; }
    }

    public class Deployment
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DeploySpec Specification { get; set; }
    }
}