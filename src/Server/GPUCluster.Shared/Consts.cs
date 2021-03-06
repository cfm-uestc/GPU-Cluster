using k8s.Models;

using System.Collections.Generic;
using System.IO;
using Docker.DotNet.Models;
using System.Linq;

namespace GPUCluster.Shared
{
    public static partial class Consts
    {
        public static readonly DirectoryInfo StaticDockerFileDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "docker"));
        public static readonly string PrivateDockerRepo = "zhongbazhu/cfm_private";
        public static readonly string PublicRootPath = Path.Combine("/tmp", "TestPublicMount");
        public static AuthConfig PrivateDockerRepoToken;
    }
}