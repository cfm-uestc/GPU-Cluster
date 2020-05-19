using System.IO;

namespace GPUCluster.Shared
{
    public static class Consts
    {
        public static readonly DirectoryInfo StaticDockerFileDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "docker"));
        public static readonly string PrivateDockerRepo = "zhongbazhu/cfm_private";
    }
}