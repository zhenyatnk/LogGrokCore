using System;
using System.IO;

namespace LogGrokX
{
    public static class PathHelpers
    {
        public static string GetLocalFilePath(string fileName) => Path.Combine(AppContext.BaseDirectory, fileName);
    }
}