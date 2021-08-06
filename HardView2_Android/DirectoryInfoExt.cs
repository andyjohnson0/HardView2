using System;
using System.Linq;
using System.IO;

namespace HardView2
{
    public static class DirectoryInfoExt
    {
        public static FileInfo First(
            this DirectoryInfo di,
            string[] fileTypes = null)
        {
            var files = di.GetFiles(fileTypes);
            return (files.Length > 0) ? files[0] : null;
        }


        public static FileInfo Last(
            this DirectoryInfo di,
            string[] fileTypes = null)
        {
            var files = di.GetFiles(fileTypes);
            return (files.Length > 0) ? files[files.Length - 1] : null;
        }


        public static FileInfo Random(
            this DirectoryInfo di,
            string[] fileTypes = null)
        {
            var files = di.GetFiles(fileTypes);
            if (files.Length > 0)
                return files[new Random().Next(0, files.Length)];
            else
                return null;
        }


        public static FileInfo[] GetFiles(
            this DirectoryInfo di,
            string[] fileTypes = null)
        {
            return di.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                            .Where(fi => fi.FullName.EndsWithAny(fileTypes))
                            .OrderBy(fi => fi.Name)
                            .ToArray();
        }


        private static bool EndsWithAny(
            this string str,
            string[] endings = null)
        {
            return (endings != null) ? endings.Any(s => str.EndsWith(s)) : true;
        }
    }
}