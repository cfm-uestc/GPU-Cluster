using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using GPUCluster.Shared.Docker;
using GPUCluster.Shared.Events;
using GPUCluster.Shared.Models.Instance;
using Newtonsoft.Json;

namespace GPUCluster.Shared.Models.Workload
{
    public class Image
    {
        public event EventHandler<JSONMessage> BuildStatusChanged;

        public Image()
        {
            BaseImageTag = "nvidia/cuda";
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ImageID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<Container> Containers { get; set; }
        [Display(Name = "Image tag")]
        public string Tag { get; set; }
        public string ImageTagSuffix { get => User == null ? Tag : Tag.StartsWith(User.UserName + "_") ? Tag.Substring((User.UserName + "_").Length) : Tag; }
        [Display(Name = "Parent Image")]
        public string BaseImageTag { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastModifiedTime { get; set; }

        private async Task<DirectoryInfo> prepareDockerFiletoBuildAsync()
        {
            if (User == null)
            {
                throw new NullReferenceException("User is null");
            }
            var directory = IOUtils.MakeDirs(Path.Combine("/tmp", "dockerBuild", User.Id, Tag));
            var copiedFiles = IOUtils.Copy(Consts.StaticDockerFileDirectory, directory, true);
            BuildStatusChanged?.Invoke(this, new JSONMessage()
            {
                Stream = "Prepare file to build"
            });
            var dockerFile = copiedFiles.Find(x => x.Name == "Dockerfile");
            string lines;
            using (var reader = dockerFile.OpenText())
            {
                lines = await reader.ReadToEndAsync();
            }
            lines = string.Format(lines, BaseImageTag, User.UserName, User.UserName);
            using (var stream = dockerFile.OpenWrite())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(lines);
                }
            }
            return directory;
        }
        public async Task<bool> CreateAndBuildAsync()
        {
            using (Invoker invoker = new Invoker())
            {
                var directory = await prepareDockerFiletoBuildAsync();
                var tarballFile = IOUtils.Tar(directory, Path.Combine(directory.FullName, "Dockerfile.tar"));
                try
                {
                    Stream buildOutput = await invoker.BuildAsync(tarballFile.FullName, new string[] { $"{Consts.PrivateDockerRepo}:{Tag}" });
                    using (var reader = new StreamReader(buildOutput))
                    {
                        while (!reader.EndOfStream)
                        {
                            var result = await reader.ReadLineAsync();
                            BuildStatusChanged?.Invoke(this, JsonConvert.DeserializeObject<JSONMessage>(result));
                        }
                    }
                    return true;
                }
                catch (DockerApiException e)
                {
                    BuildStatusChanged?.Invoke(this, new JSONMessage()
                    {
                        Error = new JSONError()
                        {
                            Message = e.Message,
                            Code = -1
                        },
                        ErrorMessage = "CRITICAL"
                    });
                    return false;
                }

            }
        }

        public async Task<bool> PushDockerImageAsync()
        {
            Progress<JSONMessage> progress = new Progress<JSONMessage>(msg =>
            {
                BuildStatusChanged?.Invoke(this, msg);
            });
            try
            {
                using (Invoker invoker = new Invoker())
                {
                    await invoker.PushAsync(Consts.PrivateDockerRepo, Tag, progress);
                }
                return true;
            }
            catch (DockerApiException e)
            {
                BuildStatusChanged?.Invoke(this, new JSONMessage()
                {
                    Error = new JSONError()
                    {
                        Message = e.Message,
                        Code = -1
                    },
                    ErrorMessage = "CRITICAL"
                });
                return false;
            }
        }
    }
}