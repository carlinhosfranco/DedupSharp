using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DedupSharp.Helpers.IO
{
    public static class BasicUtilities
    {
        public static void CheckIfDirectoryExists(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Console.WriteLine("Directory does not exist: {0}", dirName);
                Environment.Exit(0);
            }
        }

        public static void CheckIfFileExists(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist: {0}", path);
                Environment.Exit(0);
            }
        }
        public static IEnumerable<string> GetImageFilenames(string sourceDir, int maxImages)
        {
            var names = GetImageFilenamesList(sourceDir, maxImages);
            while (true)
            {
                foreach (var name in names)
                    yield return name;
            }
        }

        public static IEnumerable<string> GetImageFilenamesList(string sourceDir, int maxImages)
        {
            List<string> fileNames = new List<string>();
            var dirInfo = new DirectoryInfo(sourceDir);

            foreach (var file in dirInfo.GetFiles())
            {
                if (file.Extension.ToUpper(CultureInfo.InvariantCulture) == ".JPG")
                {
                    fileNames.Add(file.Name);
                }
            }
            return fileNames.Take(Math.Min(maxImages, fileNames.Count)).OrderBy(f => f).ToList();
        }

         /// <summary>
        /// Creates a seed that does not depend on the system clock. A unique value will be created with each invocation.
        /// </summary>
        /// <returns>An integer that can be used to seed a random generator</returns>
        /// <remarks>This method is thread safe.</remarks>
        public static int MakeRandomSeed()
        {
            return Guid.NewGuid().ToString().GetHashCode();
        }
    }
}