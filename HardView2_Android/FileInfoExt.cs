using System;
using System.Linq;
using System.IO;


namespace HardView2
{
    public static class FileInfoExt
    {
        public static FileInfo Next(
            this FileInfo fi,
            string[] fileTypes = null)
        {
            return fi.AtOffset(+1, fileTypes);
        }


        public static FileInfo Previous(
            this FileInfo fi,
            string[] fileTypes = null)
        {
            return fi.AtOffset(-1, fileTypes);
        }


        public static FileInfo AtOffset(
            this FileInfo fi,
            int offset,
            string[] fileTypes = null)
        {
            if (fi == null)
                return null;
            var files = fi.Directory.GetFiles(fileTypes);
            var i = files.IndexOf(fi);
            if (i == -1)
                return null;  // Shouldn't happen
            var j = i + offset;
            return (j >= 0 && j < files.Length) ? files[j] : files[i];
        }


        public static int IndexOf(
            this FileInfo[] files,
            FileInfo fi)
        {
            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].FullName == fi.FullName)
                    return i;
            }
            return -1;
        }
    }
}