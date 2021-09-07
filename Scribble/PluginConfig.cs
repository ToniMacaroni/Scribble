using System.Collections.Generic;
using IPA.Config.Stores.Attributes;

namespace Scribble
{
    public class PluginConfig
    {
        public bool VisibleDuringPlay { get; set; } = true;

        public bool LoadDrawingsAnimated { get; set; } = true;

        public int ThumbnailSize { get; set; } = 500;

        public string LastUsedVersion { get; set; } = "0.0.0";

        public string AutoLoadDrawing { get; set; } = "";

        [Ignore]
        public bool FirstTimeLaunch { get; set; } = false;

        public bool AnimationWaitForStableFPS { get; set; } = true;
    }
}