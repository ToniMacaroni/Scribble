using System.Reflection;
using System.Text;
using Scribble.Effects;
using Scribble.Stores;
using UnityEngine;

namespace Scribble
{
    internal class CustomBrush
    {
        public string Name { get; set; }

        public int Index { get; set; }

        private string _colorString;

        [IniParsable(Name = "Color")]
        public string ColorString
        {
            get => _colorString;
            set
            {
                _colorString = value;
                ColorUtility.TryParseHtmlString(_colorString, out var color);
                Color = color;
            }
        }

        public Color Color { get; private set; }

        [IniParsable(Name = "Texture")]
        public string TextureName { get; set; }

        [IniParsable(Name = "Effect")]
        public string EffectName { get; set; }

        [IniParsable]
        public float Size { get; set; }

        [IniParsable]
        public float Glow { get; set; }

        [IniParsable]
        public TextureModes TextureMode { get; set; }

        [IniParsable]
        public Vector2 Tiling { get; set; }

        public CustomBrush()
        {
            Name = "unnamed";
            ColorString = "#ffffff";
            TextureName = "standard";
            EffectName = "Standard";
            Size = 20;
            Glow = 1f;
            TextureMode = TextureModes.Stretch;
            Tiling = new Vector2(1f, 1f);
        }

        public Material CreateMaterial(EffectStore effects)
        {
            Effect effect = effects.GetEffect(EffectName);
            Material mat = effect.CreateMaterial(this);
            return mat;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[" + Name + "] ");
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                sb.Append(property.Name).Append(":").Append(property.GetValue(this)).Append(" ");
            }

            return sb.ToString();
        }

        public enum TextureModes
        {
            Stretch,
            Tile
        }
    }
}