using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace CacheService
{
    public class IOManager
    {
        public static void CopyDir(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyDir(diSource, diTarget);
        }

        public static void CopyDir(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                // Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDir(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void CopyFile(string sourceFilePath, string targetDir)
        {
            FileInfo diSource = new FileInfo(sourceFilePath);

            diSource.CopyTo(Path.Combine(targetDir, diSource.Name), true);
        }
    }
}
