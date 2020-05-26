using System.Threading.Tasks;
using GPUCluster.Shared.Models.Workload;

namespace GPUCluster.Shared.K8s
{
    public class Deployer
    {
        private Container _container;

        public Deployer(Container container)
        {
            _container = container;
        }
        public async Task DeployAsync()
        {

        }
    }
}