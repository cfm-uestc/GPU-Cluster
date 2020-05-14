using System;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using GPUCluster.Shared;

namespace GPUCluster.Shared
{
    public class GPUInfo
    {
        static int _totalGPUs = CudaContext.GetDeviceCount();
        public static int TotalGPUs => _totalGPUs;
        public int DeviceID { get; private set; }
        public CudaDeviceProperties DeviceProperties { get; private set; }
        public SizeT FreeVRam { get; private set; }
        public SizeT TotalVRam { get; private set; }
        public GPUInfo(int deviceID)
        {
            CudaDeviceProperties props = CudaContext.GetDeviceInfo(deviceID);
            DeviceID = deviceID;
            DeviceProperties = props;
            using (CudaContext ctx = new CudaContext())
            {
                TotalVRam = ctx.GetTotalDeviceMemorySize();
                FreeVRam = ctx.GetFreeDeviceMemorySize();
            }
        }
        public override string ToString()
        {
            SizeT usedVRam = TotalVRam - FreeVRam;
            usedVRam /= 1024 * 1024;
            SizeT totalVRam = TotalVRam / (1024 * 1024);
            return $"[{DeviceProperties.PciDeviceId}] {DeviceProperties.DeviceName} | {usedVRam} / {totalVRam} MiB";
        }
    }
}
