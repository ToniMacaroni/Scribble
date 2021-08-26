using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    public static class AssetLoader
    {
        public static AssetBundle Assets;

        public static void Load()
        {
            Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Scribble.Resources.scribbleassets");
            if (stream == null)
            {
                Plugin.Log.Debug("Couldn't load AssetBundle from Stream");
                return;
            }
            Assets = AssetBundle.LoadFromStream(stream);
            stream.Close();
        }

        public static T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            return Assets.LoadAsset<T>(name);
        }
    }
}
