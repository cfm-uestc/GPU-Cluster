using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using GPUCluster.Shared.Docker;
using GPUCluster.Shared.Events;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public class Image
    {
        public event EventHandler<StatusChangedEventArgs> BuildStatusChanged;

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
            var directory = IOUtils.MakeDirs(Path.Combine("/tmp", "dockerBuild", User.Id));
            var copiedFiles = IOUtils.Copy(Consts.StaticDockerFileDirectory, directory);
            BuildStatusChanged?.Invoke(this, new StatusChangedEventArgs()
            {
                Message = "Copy files"
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
        public async Task<Stream> CreateAndBuildAsync()
        {
            using (Invoker invoker = new Invoker())
            {
                var directory = await prepareDockerFiletoBuildAsync();
                var tarballFile = await IOUtils.Tar(directory);
                Stream result = await invoker.Build(tarballFile.FullName, new string[] { $"{Consts.PrivateDockerRepo}:{Tag}" });
                return result;
            }
        }

        public static async Task PushDockerFileAsync(string v)
        {
            await Task.Delay(5000);
        }
    }
}