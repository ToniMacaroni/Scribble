using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scribble.Effects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scribble.Stores
{
    internal class EffectStore
    {
        private readonly AssetLoader _assetLoader;
        private readonly BrushTextures _brushTextures;
        public List<Effect> EffectsList = new List<Effect>();
        
        public Material BrushDrawerMaterial { get; private set; }

        public EffectStore(AssetLoader assetLoader, BrushTextures brushTextures)
        {
            _assetLoader = assetLoader;
            _brushTextures = brushTextures;
        }

        public void LoadEffects()
        {
            EffectsList.Clear();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x=>x.BaseType==typeof(Effect)))
            {
                if(!(type.GetCustomAttribute<EffectAttribute>() is {} attr)) continue;
                var shader = _assetLoader.LoadAsset<Shader>(attr.ShaderPath);
                var effect = Activator.CreateInstance(type, shader, attr.ShaderName??type.Name.Replace("Effect", ""), _brushTextures);
                EffectsList.Add((Effect)effect);
            }
            
            BrushDrawerMaterial = _assetLoader.LoadAsset<Material>("mat_brush");
        }

        public Effect GetEffect(string name)
        {
            return EffectsList.FirstOrDefault(effect => effect.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}