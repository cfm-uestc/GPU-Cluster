using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GPUCluster.Shared
{
    public static class Extensions
    {
        public static int ToMiB(this int bytes)
        {
            return bytes / (1024 * 1024);
        }
    }

    public class IOUtils
    {
        public static DirectoryInfo MakeDirs(string path)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path);
            }
            try
            {
                return Directory.CreateDirectory(path);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        internal static List<FileInfo> Copy(DirectoryInfo sourceDirectory, DirectoryInfo destDirectory)
        {
            var copiedFiles = new List<FileInfo>();
            foreach (var file in sourceDirectory.EnumerateFiles())
            {
                copiedFiles.Add(file.CopyTo(Path.Combine(destDirectory.FullName, file.Name)));
            }
            return copiedFiles;
        }

        public static Task<FileInfo> Tar(DirectoryInfo directory)
        {
            
        }
    }
}
