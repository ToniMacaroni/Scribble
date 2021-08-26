using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using IniParser;
using IniParser.Model;
using UnityEngine;

namespace Scribble
{
    public static class Brushes
    {
        public static FileInfo File = new FileInfo("UserData\\Scribble\\CustomBrushes.ini");

        public static List<CustomBrush> BrusheList;

        public static CustomBrush EffectBrush;

        public static void WriteBrushes(List<CustomBrush> brushList)
        {
            if (!File.Exists)
            {
                File.Directory.Create();
            }

            var parser = new FileIniDataParser();
            var data = new IniData();
            foreach (CustomBrush customBrush in brushList)
            {
                WriteBrush(data, customBrush);
            }
            parser.WriteFile(File.FullName, data);
        }

        public static void LoadBrushes()
        {
            EffectBrush = new CustomBrush {ColorString = "#238fe8", TextureName = "brush"};
            if (!File.Exists)
            {
                List<CustomBrush> defaults = GetDefaultBrushes();
                BrusheList = defaults;
                WriteBrushes(defaults);
            }
            List<CustomBrush> result = new List<CustomBrush>();
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(File.FullName);
            var currentIndex = 0;
            foreach (SectionData sectionData in data.Sections)
            {
                string name = sectionData.SectionName;
                CustomBrush brush = ReadBrush(data, name);
                brush.Index = currentIndex;
                result.Add(brush);
                currentIndex++;
            }

            BrusheList = result;
        }

        private static void WriteBrush(IniData data, CustomBrush brush)
        {
            foreach (PropertyInfo propertyInfo in typeof(CustomBrush).GetProperties())
            {
                IniParsable parsable = propertyInfo.GetCustomAttribute<IniParsable>();
                if (parsable != null)
                {
                    var keyName = !string.IsNullOrEmpty(parsable.Name) ? parsable.Name : propertyInfo.Name;
                    data[brush.Name][keyName] = propertyInfo.GetValue(brush).ToString();
                }
            }
        }

        private static CustomBrush ReadBrush(IniData data, string name)
        {
            CustomBrush brush = new CustomBrush();
            foreach (PropertyInfo propertyInfo in typeof(CustomBrush).GetProperties())
            {
                IniParsable parsable = propertyInfo.GetCustomAttribute<IniParsable>();
                if (parsable!=null)
                {
                    var keyName = !string.IsNullOrEmpty(parsable.Name) ? parsable.Name : propertyInfo.Name;
                    if(!data[name].ContainsKey(keyName))continue;
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(brush, data[name][keyName]);
                    }else if (propertyInfo.PropertyType == typeof(float))
                    {
                        if(float.TryParse(data[name][keyName], out var value))propertyInfo.SetValue(brush, value);
                    }else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        if(bool.TryParse(data[name][keyName], out var value))propertyInfo.SetValue(brush, value);
                    }else if (propertyInfo.PropertyType == typeof(Vector2))
                    {
                        if(ParsingExtensions.ParseVector2(data[name][keyName], out var value)) propertyInfo.SetValue(brush, value);
                    }else if (propertyInfo.PropertyType.IsEnum)
                    {
                        try
                        {
                            object value = Enum.Parse(propertyInfo.PropertyType, data[name][keyName], true);
                            propertyInfo.SetValue(brush, value);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }

            brush.Name = name;
            return brush;
        }

        public static List<CustomBrush> GetDefaultBrushes()
        {
            List<CustomBrush> result = new List<CustomBrush>();

            CustomBrush brush = new CustomBrush() {Name = "Primary", ColorString = "#e63535", TextureName = "brush", Glow = 0.8f};
            result.Add(brush);

            brush = new CustomBrush() {Name = "Secondary", ColorString = "#238fe8", TextureName = "brush", Glow = 0.8f };
            result.Add(brush);

            return result;
        }
    }

    public class CustomBrush
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
            EffectName = "standard";
            Size = 20;
            Glow = 1f;
            TextureMode = TextureModes.Stretch;
            Tiling = new Vector2(1f, 1f);
        }

        public Material CreateMaterial()
        {
            Effect effect = Effects.GetEffect(EffectName);
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

    public class IniParsable : Attribute
    {
        public string Name;
    }
}
