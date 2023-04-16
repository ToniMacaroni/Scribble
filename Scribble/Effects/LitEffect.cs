using Scribble.Helpers;
using Scribble.Stores;
using UnityEngine;

namespace Scribble.Effects
{
    [Effect("CustomSimpleLitTube.shader")]
    internal class LitEffect : Effect
    {
        public LitEffect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            MaterialPropertyHelper.SetProperties(mat, brush.CustomEffectProperties);
            return mat;
        }
    }
}