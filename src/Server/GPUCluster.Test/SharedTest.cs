using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPUCluster.Shared;
using System.Diagnostics;

namespace GPUCluster.Test
{
    [TestClass]
    public class SharedTest
    {
        [TestMethod]
        public void TestGPUInfo()
        {
            Assert.IsTrue(GPUInfo.TotalGPUs > 0);
            GPUInfo gpu = new GPUInfo(0);
            Trace.WriteLine(gpu);
        }
    }
}
