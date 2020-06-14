using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace GPUCluster.Shared
{
    public static class Extensions
    {
        public static int ToMiB(this int bytes)
        {
            return bytes / (1024 * 1024);
        }
    }

    public static class IOUtils
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

        public static List<FileInfo> Copy(this DirectoryInfo sourceDirectory, DirectoryInfo destDirectory, bool overwrite)
        {
            var copiedFiles = new List<FileInfo>();
            foreach (var file in sourceDirectory.EnumerateFiles())
            {
                if (!overwrite && File.Exists(Path.Combine(destDirectory.FullName, file.Name)))
                {
                    copiedFiles.Add(new FileInfo(Path.Combine(destDirectory.FullName, file.Name)));
                    continue;
                }
                copiedFiles.Add(file.CopyTo(Path.Combine(destDirectory.FullName, file.Name), true));
            }
            foreach (var dir in sourceDirectory.EnumerateDirectories())
            {
                if (!Directory.Exists(Path.Combine(destDirectory.FullName, dir.Name)))
                    destDirectory.CreateSubdirectory(dir.Name);
                copiedFiles.AddRange(Copy(dir, new DirectoryInfo(Path.Combine(destDirectory.FullName, dir.Name)), overwrite));
            }
            return copiedFiles;
        }

        public static long TotalSize(DirectoryInfo directory)
        {
            long folderSize = 0L;
            try
            {
                //Checks if the path is valid or not
                if (!directory.Exists)
                    return folderSize;
                else
                {
                    try
                    {
                        foreach (FileInfo file in directory.GetFiles())
                        {
                            folderSize += file.Length;
                        }

                        foreach (DirectoryInfo dir in directory.GetDirectories())
                            folderSize += TotalSize(dir);
                    }
                    catch (NotSupportedException e)
                    {
                        Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
            }
            return folderSize;
        }

        public static string ReadString(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                throw new ArgumentException($"{nameof(path)} is invalid");
            }
        }

        internal static DirectoryInfo AddDirectoryIfNotExists(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return new DirectoryInfo(dirPath);
            }
            return Directory.CreateDirectory(dirPath);
        }

        public static FileInfo TarGZ(DirectoryInfo directory, string destTarPath)
        {
            return TarGZ(directory.FullName, destTarPath);
        }

        public static FileInfo TarGZ(string sourceDirectory, string destTarPath)
        {
            if (File.Exists(destTarPath))
            {
                File.Delete(destTarPath);
            }
            using (Stream outStream = File.Create(destTarPath))
            using (Stream gzoStream = new GZipOutputStream(outStream))
            using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
            {
                tarArchive.SetRootPath(sourceDirectory);

                addDirectoryFilesToTar(tarArchive, sourceDirectory, true);
                return new FileInfo(destTarPath);
            }
        }

        private static void SetRootPath(this TarArchive tarArchive, string sourceDirectory)
        {
            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);
            // *NIX path style, when create tarEntry, the starting / is ignored
            if (tarArchive.RootPath.StartsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(0, 1);
        }

        private static void addDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            // if (rootDirectory != null && rootDirectory != "/")
            //     tarEntry.Name = tarEntry.Name.Replace(rootDirectory, ".");
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                if (filename.EndsWith(".gz") || filename.EndsWith(".tar"))
                    continue;
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                // if (rootDirectory != null && rootDirectory != "/")
                //     tarEntry.Name = tarEntry.Name.Replace(rootDirectory, ".");
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    addDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }


        internal static Stream TarGZInMemory(DirectoryInfo directory, MemoryStream streamInMem)
        {
            var sourceDirectory = directory.FullName;
            using (GZipOutputStream gzoStream = new GZipOutputStream(streamInMem))
            using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
            {
                gzoStream.IsStreamOwner = false;
                tarArchive.SetRootPath(sourceDirectory);

                addDirectoryFilesToTar(tarArchive, sourceDirectory, true);
                return streamInMem;
            }
        }
        public static void UnTarGZ(MemoryStream stream)
        {
            using (GZipInputStream gzipStream = new GZipInputStream(stream))
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
            {
                gzipStream.IsStreamOwner = false;
                tarArchive.ExtractContents("/tmp/targztest");
            }
        }
        public static void UnTarGZ(FileInfo file)
        {
            using (Stream stream = File.OpenRead(file.FullName))
            using (GZipInputStream gzipStream = new GZipInputStream(stream))
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
            {
                tarArchive.ExtractContents("/tmp/targztest");
            }
        }


        public static FileInfo Tar(DirectoryInfo directory, string destTarPath)
        {
            return Tar(directory.FullName, destTarPath);
        }


        public static FileInfo Tar(string sourceDirectory, string destTarPath)
        {
            if (File.Exists(destTarPath))
            {
                File.Delete(destTarPath);
            }
            using (Stream outStream = File.Create(destTarPath))
            using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(outStream))
            {
                tarArchive.SetRootPath(sourceDirectory);

                addDirectoryFilesToTar(tarArchive, sourceDirectory, true);
                return new FileInfo(destTarPath);
            }
        }
        internal static Stream TarInMemory(DirectoryInfo directory, MemoryStream streamInMem)
        {
            var sourceDirectory = directory.FullName;
            using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(streamInMem))
            {
                tarArchive.IsStreamOwner = false;
                tarArchive.SetRootPath(sourceDirectory);

                addDirectoryFilesToTar(tarArchive, sourceDirectory, true);
                return streamInMem;
            }
        }
    }
}
