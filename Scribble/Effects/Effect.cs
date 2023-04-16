using System;
using Scribble.Stores;
using UnityEngine;

namespace Scribble.Effects
{
    internal class Effect
    {
        public string Name;
        public Shader Shader;

        protected readonly BrushTextures BrushTextures;

        public Effect(Shader shader, string name, BrushTextures brushTextures)
        {
            Shader = shader;
            Name = name;
            BrushTextures = brushTextures;
        }

        public virtual Material CreateMaterial(CustomBrush brush)
        {
            var mat = new Material(Shader);
            mat.SetColor("_Color", brush.Color);
            var tex = BrushTextures.GetTexture(brush.TextureName);
            if (tex) mat.SetTexture("_Tex", tex);
            mat.SetVector("_Tiling", brush.Tiling);
            mat.SetFloat("_Glow", brush.Glow);
            return mat;
        }
    }

    internal class EffectAttribute : Attribute
    {
        public string ShaderPath { get; }
        public string ShaderName { get; }


        public EffectAttribute(string shaderPath, string shaderName = null)
        {
            ShaderPath = shaderPath;
            ShaderName = shaderName;
        }
    }
}