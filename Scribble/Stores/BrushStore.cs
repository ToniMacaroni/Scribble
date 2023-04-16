using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IniParser;
using IniParser.Model;
using Scribble.Helpers;
using UnityEngine;

namespace Scribble.Stores
{
    internal class BrushStore : IDisposable
    {
        public FileInfo File;

        public List<CustomBrush> BrushList;

        public CustomBrush EffectBrush;

        public BrushStore(PluginDirectories dirs)
        {
            File = dirs.DataDir.GetFile("CustomBrushes.ini");
        }

        public void WriteBrushes(List<CustomBrush> brushList)
        {
            if (!File.Exists)
            {
                File.Directory?.Create();
            }

            var parser = new FileIniDataParser();
            var data = new IniData();

            foreach (CustomBrush customBrush in brushList)
            {
                WriteBrush(data, customBrush);
            }

            parser.WriteFile(File.FullName, data);
        }

        public void LoadBrushes()
        {
            EffectBrush = new CustomBrush { ColorString = "#238fe8", TextureName = "brush" };
            if (!File.Exists)
            {
                BrushList = GetDefaultBrushes();
                WriteBrushes(BrushList);
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

            BrushList = result;
        }

        private void WriteBrush(IniData data, CustomBrush brush)
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

            foreach (var effectProperty in brush.CustomEffectProperties)
            {
                data[brush.Name][effectProperty.Key] = effectProperty.Value;
            }
        }

        private CustomBrush ReadBrush(IniData data, string name)
        {
            CustomBrush brush = new CustomBrush();

            var registeredProps = new List<string>();
            
            foreach (PropertyInfo propertyInfo in typeof(CustomBrush).GetProperties())
            {
                IniParsable parsable = propertyInfo.GetCustomAttribute<IniParsable>();
                if (parsable != null)
                {
                    var keyName = !string.IsNullOrEmpty(parsable.Name) ? parsable.Name : propertyInfo.Name;
                    if (!data[name].ContainsKey(keyName)) continue;
                    
                    registeredProps.Add(keyName);
                    
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(brush, data[name][keyName]);
                    }
                    else if (propertyInfo.PropertyType == typeof(float))
                    {
                        if (float.TryParse(data[name][keyName], out var value)) propertyInfo.SetValue(brush, value);
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        if (bool.TryParse(data[name][keyName], out var value)) propertyInfo.SetValue(brush, value);
                    }
                    else if (propertyInfo.PropertyType == typeof(Vector2))
                    {
                        if (ParsingExtensions.ParseVector2(data[name][keyName], out var value)) propertyInfo.SetValue(brush, value);
                    }
                    else if (propertyInfo.PropertyType.IsEnum)
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

            var customEffectsData = new Dictionary<string, string>();

            foreach (var keyData in data[name])
            {
                if(registeredProps.Contains(keyData.KeyName)) continue;
                customEffectsData.Add(keyData.KeyName, keyData.Value);
            }
            
            brush.CustomEffectProperties = customEffectsData;
            brush.Name = name;
            return brush;
        }

        public List<CustomBrush> GetDefaultBrushes()
        {
            List<CustomBrush> result = new List<CustomBrush>();

            CustomBrush brush = new CustomBrush { Name = "Primary", ColorString = "#e63535", TextureName = "brush", Glow = 0.8f };
            result.Add(brush);

            brush = new CustomBrush { Name = "Secondary", ColorString = "#238fe8", TextureName = "brush", Glow = 0.8f };
            result.Add(brush);

            return result;
        }

        public void Dispose()
        {
            WriteBrushes(BrushList);
        }
    }

    public class IniParsable : Attribute
    {
        public string Name;
    }
}