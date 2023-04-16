using Scribble.Stores;
using UnityEngine;

namespace Scribble.Effects
{
    internal static class LightEffectHelper
    {
        private static readonly int GlowMultiplier1 = Shader.PropertyToID("_GlowMultiplier1");
        private static readonly int GlowMultiplier2 = Shader.PropertyToID("_GlowMultiplier2");
        
        public static void InitMaterial(Material mat, CustomBrush brush)
        {
            // when color is coming from light
            mat.SetFloat(GlowMultiplier1, 3f);
            // when using custom color
            mat.SetFloat(GlowMultiplier2, 3f);
        }
    }

    [Effect("Light.shader")]
    internal class Light1Effect : Effect
    {
        private const int Id = 0;
        
        private static readonly int IdProp = Shader.PropertyToID("_Id");

        public Light1Effect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat(IdProp, Id);
            LightEffectHelper.InitMaterial(mat, brush);
            return mat;
        }
    }
    
    [Effect("Light.shader")]
    internal class Light2Effect : Effect
    {
        private const int Id = 1;
        
        private static readonly int IdProp = Shader.PropertyToID("_Id");

        public Light2Effect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat(IdProp, Id);
            LightEffectHelper.InitMaterial(mat, brush);
            return mat;
        }
    }
    
    [Effect("Light.shader")]
    internal class Light3Effect : Effect
    {
        private const int Id = 2;
        
        private static readonly int IdProp = Shader.PropertyToID("_Id");

        public Light3Effect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat(IdProp, Id);
            LightEffectHelper.InitMaterial(mat, brush);
            return mat;
        }
    }
    
    [Effect("Light.shader")]
    internal class Light4Effect : Effect
    {
        private const int Id = 3;
        
        private static readonly int IdProp = Shader.PropertyToID("_Id");

        public Light4Effect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat(IdProp, Id);
            LightEffectHelper.InitMaterial(mat, brush);
            return mat;
        }
    }
    
    [Effect("Light.shader")]
    internal class Light5Effect : Effect
    {
        private const int Id = 4;
        
        private static readonly int IdProp = Shader.PropertyToID("_Id");

        public Light5Effect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat(IdProp, Id);
            LightEffectHelper.InitMaterial(mat, brush);
            return mat;
        }
    }
    
    [Effect("Light.shader")]
    internal class Light6Effect : Effect
    {
        private const int Id = 5;
        
        private static readonly int IdProp = Shader.PropertyToID("_Id");

        public Light6Effect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat(IdProp, Id);
            LightEffectHelper.InitMaterial(mat, brush);
            return mat;
        }
    }
}