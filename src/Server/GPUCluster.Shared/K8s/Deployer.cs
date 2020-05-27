using System.Threading.Tasks;
using GPUCluster.Shared.Models.Workload;
using k8s;

namespace GPUCluster.Shared.K8s
{
    public class Deployer
    {
        private readonly Kubernetes _client;
        public Deployer(Container container)
        {
            KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            _client = new Kubernetes(config);
        }
        public async Task DeployAsync()
        {

        }
    }
}