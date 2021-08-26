using System.Collections.Generic;

namespace Scribble
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public bool VisibleDuringPlay { get; set; } = true;

        public int ThumbnailSize { get; set; } = 500;

        public string LastUsedVersion { get; set; } = "0.0.0";
    }
}