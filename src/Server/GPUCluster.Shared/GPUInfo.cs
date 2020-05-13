using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace GPUCluster.Shared
{
    public class GPUInfo
    {
        static int _totalGPUs = CudaContext.GetDeviceCount();
        public static int TotalGPUs => _totalGPUs;
        public GPUInfo(int deviceID)
        {
            CudaDeviceProperties props = CudaContext.GetDeviceInfo(0);
        }
    }
}
