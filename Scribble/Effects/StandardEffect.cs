using Scribble.Stores;
using UnityEngine;

namespace Scribble.Effects
{
    [Effect("simple.shader", "Standard")]
    internal class StandardEffect : Effect
    {
        public StandardEffect(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);

            return mat;
        }
    }
}