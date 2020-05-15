using System;
using GPUCluster.Shared;

namespace GPUCluster.WebService.Models
{
    public class IndexViewModel
    {
        public string CurrentGPUInfo { get; set; }
        public IndexViewModel()
        {
            CurrentGPUInfo = new GPUInfo(0).ToString();
        }
    }
}
