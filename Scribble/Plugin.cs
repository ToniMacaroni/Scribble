using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using Scribble.Installers;
using Scribble.UI;
using SemVer;
using SiraUtil.Zenject;
using TMPro;
using UnityEngine;
using Config = IPA.Config.Config;
using Logger = IPA.Logging.Logger;
using Version = SemVer.Version;

namespace Scribble
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {

        //public static ScribbleUI UI;
        public Version Version { get; set; }
        public static bool FirstTimeLaunch;
        public static GameObject ScreenSystem;
        public static bool Activated;

        private static bool _drawingEnabled;

        //public static bool DrawingEnabled
        //{
        //    get => _drawingEnabled;
        //    set
        //    {
        //        UI.Shaded = !value;
        //        ScribbleContainer.DrawingEnabled = value;
        //        GlobalBrushManager.BrushesEnabled = value;
        //        if(ScreenSystem!=null) ScreenSystem.SetActive(!value);
        //        var bg = GameObject.Find("PauseMenu/Wrapper/UI/BG");
        //        var canvas = GameObject.Find("PauseMenu/Wrapper/UI/Canvas");
        //        if (bg&&canvas){ bg.SetActive(!value); canvas.SetActive(!value);}
        //        _drawingEnabled = value;
        //    }
        //}

        [Init]
        public Plugin(Logger logger, Config config, Zenjector zenjector)
        {
            PluginConfig.Instance = config.Generated<PluginConfig>();

            zenjector.OnApp<PluginAppInstaller>().WithParameters(logger);
            zenjector.OnMenu<PluginMenuInstaller>();


            var asm = Assembly.GetExecutingAssembly();
            var version = asm.GetName().Version;
            Version = new Version(version.Major, version.Minor, version.Build);

        }

        //[OnStart]
        //public void OnStart()
        //{
        //    if (PluginConfig.Instance.LastUsedVersion == "0.0.0") FirstTimeLaunch = true;
        //    PluginConfig.Instance.LastUsedVersion = Version.ToString();

        //    Effects.LoadEffects();
        //    BrushTextures.LoadTextures();
        //    Brushes.LoadBrushes();

        //    BS_Utils.Utilities.BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
        //    BS_Utils.Utilities.BSEvents.gameSceneLoaded += OnGameSceneLoaded;
        //    BS_Utils.Utilities.BSEvents.songPaused += OnSongPause;
        //    BS_Utils.Utilities.BSEvents.songUnpaused += OnSongUnpause;
        //    BS_Utils.Utilities.BSEvents.menuSceneLoaded += OnMenuSceneLoaded;
        //}

        //[OnExit]
        //public void OnExit()
        //{
        //    Brushes.WriteBrushes(Brushes.BrusheList);
        //}

        //private void OnMenuSceneLoaded()
        //{
        //    if(UI)UI.Show(true);
        //    ShowContainerIfNeeded(true);
        //}

        //private void OnSongUnpause()
        //{
        //    if(UI)UI.Show(false);
        //    ShowContainerIfNeeded(false);
        //}

        //private void OnSongPause()
        //{
        //    if(UI)UI.Show(true);
        //    ShowContainerIfNeeded(true);
        //}

        //private void OnMenuSceneLoadedFresh()
        //{
        //    if (Activated) FirstTimeLaunch = false;
        //    ScribbleContainer.Create();

        //    if(UI)UnityEngine.Object.Destroy(UI.gameObject);
        //    UI = ScribbleUI.Create();

        //    ScreenSystem = GameObject.Find("ScreenSystem");

        //    DrawingEnabled = false;
        //    Activated = true;
        //}

        //private void OnGameSceneLoaded()
        //{
        //    if (UI) UI.Show(false);
        //    ShowContainerIfNeeded(false);
        //    GameplayManager.OnGameSceneLoaded();
        //}

        //private void ShowContainerIfNeeded(bool show)
        //{
        //    if (ScribbleContainer.Instance && !PluginConfig.Instance.VisibleDuringPlay)
        //    {
        //        if(show) ScribbleContainer.Instance.Show();
        //        else ScribbleContainer.Instance.Hide();
        //    }
        //}

        public bool IsVersionNewerThanLastUsed()
        {
            var configVersion = new Version(PluginConfig.Instance.LastUsedVersion);
            return new Range($">{configVersion}").IsSatisfied(Version);
        }

    }
}
