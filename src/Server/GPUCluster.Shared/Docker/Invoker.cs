using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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

        public async Task<IList<ContainerListResponse>> PsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _client.Containers.ListContainersAsync(new ContainersListParameters()
            {
                Limit = 10,
            }, cancellationToken);
        }

        public async Task<CommitContainerChangesResponse> CommitAsync(string containerID, string repo, string tag, string comment = null, string author = null, IList<string> changes = null, bool? pause = null, CancellationToken cancellationToken = default(CancellationToken))
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
            }, cancellationToken);
        }

        public async Task<Stream> BuildAsync(string dockerTarballPath, IList<string> tags, CancellationToken cancellationToken = default(CancellationToken))
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
                }, cancellationToken);
            }
        }
        public async Task<Stream> BuildAsync(Stream tarStream, IList<string> tags, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _client.Images.BuildImageFromDockerfileAsync(tarStream, new ImageBuildParameters()
            {
                Tags = tags
            }, cancellationToken);
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
        }

        public async Task PushAsync(string imageName, string tag, IProgress<JSONMessage> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _client.Images.PushImageAsync(imageName, new ImagePushParameters()
            {
                Tag = tag
            }, Consts.PrivateDockerRepoToken, progress, cancellationToken);
        }
    }
}