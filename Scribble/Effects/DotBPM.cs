using Scribble.Stores;
using UnityEngine;

namespace Scribble.Effects
{
    [Effect("DotBPM.shader", "DotBPM")]
    internal class DotBPM : Effect
    {
        public DotBPM(Shader shader, string name, BrushTextures brushTextures) : base(shader, name, brushTextures)
        {
        }

        public override Material CreateMaterial(CustomBrush brush)
        {
            var mat = base.CreateMaterial(brush);
            mat.SetFloat("_Speed", 1f);
            mat.SetColor("_Color2", brush.Color);
            return mat;
        }
    }
}