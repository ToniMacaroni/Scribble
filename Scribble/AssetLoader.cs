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
    public class AssetLoader : IDisposable
    {
        private AssetBundle _assetBundle;

        public bool Load()
        {
            Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Scribble.Resources.scribbleassets");
            if (stream == null)
            {
                Debug.LogError("Couldn't load AssetBundle from Stream");
                return false;
            }
            _assetBundle = AssetBundle.LoadFromStream(stream);
            stream.Close();
            return true;
        }

        public T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            return _assetBundle.LoadAsset<T>(name);
        }

        public void Dispose()
        {
            _assetBundle.Unload(true);
        }
    }
}
