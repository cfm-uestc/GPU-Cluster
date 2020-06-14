using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GPUCluster.Shared.Models.Workload;

using k8s;
using k8s.Models;

namespace GPUCluster.Shared.K8s
{
    public class Deployer
    {
        private readonly Kubernetes _client;
        public Deployer()
        {
            KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            _client = new Kubernetes(config);
        }
        public async Task DeployAsync(Container container)
        {
            V1Deployment deployment = getDeploymentFromContainer(container);
        }

        private V1Deployment getDeploymentFromContainer(Container container)
        {
            return new V1Deployment(
                       metadata: new V1ObjectMeta(
                           name: container.Name,
                           labels: new Dictionary<string, string>
                           {
                               { "app", "gpu-cluster" }
                           }),
                       spec: new V1DeploymentSpec(
                           selector: new V1LabelSelector(
                               matchLabels: new Dictionary<string, string>
                               {
                                { "app", "gpu-cluster" }
                               }),
                           template: new V1PodTemplateSpec(
                               metadata: new V1ObjectMeta(
                                   labels: new Dictionary<string, string>
                                   {
                                       { "app", "gpu-cluster" }
                                   }),
                               spec: new V1PodSpec(
                                   containers: new V1Container[]
                                   {
                                       new V1Container(
                                           image: container.Image.Tag,
                                           name: container.Name,
                                           resources: new V1ResourceRequirements(
                                               limits: new Dictionary<string, ResourceQuantity>
                                               {
                                                   { "nvidia.com/gpu", new ResourceQuantity("2") }
                                               }),
                                           env: new V1EnvVar[]
                                           {
                                               new V1EnvVar("NVIDIA_VISIBLE_DEVICES", value: null)
                                           },
                                           ports: Consts.K8sContainerPortALL,
                                           volumeMounts: getMountsFromContainer(container))
                                   },
                                   imagePullSecrets: new V1LocalObjectReference[]
                                   {
                                       new V1LocalObjectReference(name: "regcred")
                                   },
                                   volumes: getVolumesFromContainer(container),
                                   securityContext: new V1PodSecurityContext(
                                       fsGroup: container.User.LinuxUser.ActualUID,
                                       runAsGroup: container.User.LinuxUser.ActualUID,
                                       runAsUser: container.User.LinuxUser.ActualUID))),
                           strategy: new V1DeploymentStrategy(type: "Recreate")));
        }

        private IList<V1Volume> getVolumesFromContainer(Container container)
        {
            throw new NotImplementedException();
        }

        private IList<V1VolumeMount> getMountsFromContainer(Container container)
        {
            throw new NotImplementedException();
        }
    }
}