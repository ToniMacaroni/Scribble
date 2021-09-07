using System.IO;
using IPA.Utilities;

namespace Scribble
{
    internal class PluginDirectories
    {
        public DirectoryInfo DataDir { get; }
        public DirectoryInfo DrawingsDir { get; }

        public PluginDirectories()
        {
            DataDir = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, "Scribble"));
            DataDir.Create();

            DrawingsDir = DataDir.CreateSubdirectory("Drawings");
        }
    }
}