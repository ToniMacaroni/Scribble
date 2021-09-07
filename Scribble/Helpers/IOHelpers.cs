using System.IO;

namespace Scribble.Helpers
{
    internal static class IOHelpers
    {
        public static FileInfo GetFile(this DirectoryInfo dir, string fileName)
        {
            return new FileInfo(Path.Combine(dir.FullName, fileName));
        }
    }
}