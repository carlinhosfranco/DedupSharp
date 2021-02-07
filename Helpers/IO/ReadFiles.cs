using System;
using System.IO;

namespace DedupSharp.Helpers.IO
{
    public static class ReadFiles
    {
        public static string[] Get(string path)
        {
            if (Directory.Exists(path))            
                return Directory.GetFiles(path);
            else
                return null;
        }        
    }
}