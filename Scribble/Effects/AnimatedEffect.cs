using Scribble.Stores;
using UnityEngine;

namespace Scribble.Effects
{
    [Effect("animated.shader", "Animated")]
    internal class AnimatedEffect : Effect
    {
        public AnimatedEffect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat("_Speed", 1f);
            return mat;
        }
    }
}