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
    internal static class Effects
    {
        public static List<Effect> EffectsList = new List<Effect>();

        public static void LoadEffects()
        {
            EffectsList.Clear();
            if (AssetLoader.Assets == null)
            {
                Plugin.Log.Debug("Failed to load effects: Assets not loaded");
                return;
            }
            StandardEffect standardEffect = new StandardEffect{Name = "standard", Shader = AssetLoader.LoadAsset<Shader>("assets/shaders/simple.shader") };
            AnimatedEffect animatedEffect = new AnimatedEffect{ Name = "animated", Shader = AssetLoader.LoadAsset<Shader>("assets/shaders/animated.shader") };
            DotBPM dotBPM = new DotBPM{ Name = "dotbpm", Shader = AssetLoader.LoadAsset<Shader>("assets/shaders/DotBPM.shader") };
            EffectsList.Add(standardEffect);
            EffectsList.Add(animatedEffect);
            EffectsList.Add(dotBPM);
        }

        public static Effect GetEffect(string name)
        {
            return EffectsList.FirstOrDefault(effect => effect.Name == name);
        }
    }

    internal class Effect
    {
        public string Name;
        public Shader Shader;

        public virtual Material CreateMaterial(CustomBrush brush)
        {
            Material mat = new Material(Shader);
            mat.SetColor("_Color", brush.Color);
            Texture2D tex = BrushTextures.GetTexture(brush.TextureName);
            if(tex!=null) mat.SetTexture("_Tex", tex);
            mat.SetVector("_Tiling", brush.Tiling);
            mat.SetFloat("_Glow", brush.Glow);
            return mat;
        }
    }

    internal class StandardEffect : Effect
    {
        public override Material CreateMaterial(CustomBrush brush)
        {
            Material mat = base.CreateMaterial(brush);

            return mat;
        }
    }

    internal class AnimatedEffect : Effect
    {
        public override Material CreateMaterial(CustomBrush brush)
        {
            Material mat = base.CreateMaterial(brush);
            mat.SetFloat("_Speed", 1f);
            return mat;
        }
    }

    internal class DotBPM : Effect
    {
        public override Material CreateMaterial(CustomBrush brush)
        {
            Material mat = base.CreateMaterial(brush);
            mat.SetFloat("_Speed", 1f);
            mat.SetColor("_Color2", brush.Color);
            return mat;
        }
    }
}
