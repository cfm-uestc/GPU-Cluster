using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPUCluster.Shared;
using System.Diagnostics;
using GPUCluster.Shared.Docker;
using System.Threading.Tasks;
using System.IO;

namespace GPUCluster.Test
{
    [TestClass]
    public class SharedTest
    {
        private TestContext context;
        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }

        [TestMethod]
        public void TestGPUInfo()
        {
            Assert.IsTrue(GPUInfo.TotalGPUs > 0);
            GPUInfo gpu = new GPUInfo(0);
            Trace.WriteLine(gpu);
        }
        [TestMethod]
        public async Task TestDockerClientOpen()
        {
            using (Invoker invoker = new Invoker())
            {
                var containers = await invoker.Ps();
                Assert.IsTrue(containers.Count > 0);
                TestContext.WriteLine(containers[0].Image);
            }
        }
        [TestMethod]
        public async Task TestDockerBuild()
        {
            using (Invoker invoker = new Invoker())
            {
                Stream result = await invoker.Build("/home/zhuxiaosu/GPU-Cluster/src/docker/Dockerfile.tar.gz", new string[] { "gpu_cluster/aspnet_test:tag" });
                Assert.IsNotNull(result);
            }
        }
    }
}
