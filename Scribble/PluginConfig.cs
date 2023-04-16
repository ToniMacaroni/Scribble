using System;
using System.Collections.Generic;
using IPA.Config.Stores.Attributes;

namespace Scribble
{
    public class PluginConfig
    {
        // Is the drawing visible during gameplay
        public bool VisibleDuringPlay { get; set; } = true;

        // Should the drawings be loaded in with animation
        public bool LoadDrawingsAnimated { get; set; } = true;

        // Resolution of the thumbnails
        public int ThumbnailSize { get; set; } = 500;

        // Ignore, The last version the plugin was used on, automatically updated
        public string LastUsedVersion { get; set; } = "0.0.0";

        // Here you can specify the path to a drawing that will be loaded on startup
        public string AutoLoadDrawing { get; set; } = "";
        
        // How much the drawing will move when using the move tool
        public float MoveToolMultiplier { get; set; } = 2.5f;

        // How much the drawing will scale when using the rotate tool
        public float ScaleToolMultiplier { get; set; } = 5f;
        
        // How much the drawing lines will scale when using the scale line width tool
        public float ScaleLineWidthToolMultiplier { get; set; } = 0.2f;

        [Ignore]
        public bool FirstTimeLaunch { get; set; } = false;
        
        [Ignore]
        public bool FPFCEnabled { get; set; } = Environment.CommandLine.Contains("fpfc");

        // Waits for a stable FPS before rendering the drawing
        public bool AnimationWaitForStableFPS { get; set; } = true;
    }
}