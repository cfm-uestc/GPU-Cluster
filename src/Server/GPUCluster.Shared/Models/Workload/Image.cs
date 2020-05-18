using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using GPUCluster.Shared.Docker;
using GPUCluster.Shared.Models.Instance;

namespace GPUCluster.Shared.Models.Workload
{
    public class Image
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ImageID { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<Container> Containers { get; set; }
        [Display(Name="Built image tag")]
        public string Tag { get; set; }
        public string ImageTagSuffix { get => User == null ? Tag : Tag.StartsWith(User.UserName + "_") ? Tag.Substring((User.UserName + "_").Length) : Tag; }
        [Display(Name="Parent Docker image based-on")]
        public string BaseImageTag { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastModifiedTime { get; set; }

        private static readonly string _baseDockerFile =
            "FROM nvidia/cuda:latest\r\n";// +

        // "RUN apt-get update && apt-get install -y apt-utils\r\n" +
        // "RUN apt-get install -y openssh-server sudo vim nano\r\n" +

        // "COPY user.sh /usr/local/bin/user.sh\r\n" +
        // "RUN chmod +x /usr/local/bin/user.sh\r\n" +
        // "RUN /usr/local/bin/user.sh\r\n" +
        // "RUN rm /usr/local/bin/user.sh\r\n" +

        // "COPY entrypoint.sh /usr/local/bin/entrypoint.sh\r\n" +
        // "RUN chmod +x /usr/local/bin/entrypoint.sh\r\n" +

        // "EXPOSE 22\r\n" +
        // "ENTRYPOINT [\"/usr/local/bin/entrypoint.sh\"]\r\n" +
        // "CMD tail -f /dev/null\r\n";

        public async Task<Stream> CreateAndBuildAsync()
        {
            Invoker invoker = new Invoker();
            Stream result = await invoker.Build("/home/zhuxiaosu/GPU-Cluster/src/docker/Dockerfile.tar.gz", new string[] { Tag });
            return result;
        }

        public static async Task PushDockerFileAsync(string v)
        {
            await Task.Delay(5000);
        }
    }
}