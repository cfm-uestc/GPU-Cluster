using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace GPUCluster.Shared.Docker
{
    public class Invoker : IDisposable
    {
        private readonly DockerClient _client;
        public Invoker()
        {
            _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        public async Task<IList<ContainerListResponse>> Ps()
        {
            return await _client.Containers.ListContainersAsync(new ContainersListParameters()
            {
                Limit = 10,
            });
        }

        public async Task<CommitContainerChangesResponse> Commit(string containerID, string repo, string tag, string comment = null, string author = null, IList<string> changes = null, bool? pause = null)
        {
            return await _client.Images.CommitContainerChangesAsync(new CommitContainerChangesParameters()
            {
                RepositoryName = repo,
                Tag = tag,
                ContainerID = containerID,
                Comment = comment,
                Author = author,
                Changes = changes,
                Pause = pause
            });
        }

        public async Task<Stream> Build(string dockerTarballPath, IList<string> tags)
        {
            if (string.IsNullOrWhiteSpace(dockerTarballPath))
            {
                throw new ArgumentNullException(nameof(dockerTarballPath));
            }
            using (FileStream stream = File.OpenRead(dockerTarballPath))
            {
                return await _client.Images.BuildImageFromDockerfileAsync(stream, new ImageBuildParameters()
                {
                    Tags = tags
                });
            }
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
        }
    }
}