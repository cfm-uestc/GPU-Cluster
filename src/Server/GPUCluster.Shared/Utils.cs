using System;

namespace GPUCluster.Shared
{
    public static class Extensions
    {
        public static int ToMiB(this int bytes)
        {
            return bytes / (1024 * 1024);
        }
    }
}
