using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scheduler.Models
{
    public class GPUSPec
    {
        public string Name { get; set; }
        public int TotalVRAM { get; set; }
        public int UsedVRAM { get; set; }
        public bool Allocatable { get; set; }
    }
    public class NodeSpec
    {
        public int CPUCores { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        // public List<GPUSPec> GPUs { get; set; }
    }

    public class Node
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        // public NodeSpec Specification { get; set; }
    }
}