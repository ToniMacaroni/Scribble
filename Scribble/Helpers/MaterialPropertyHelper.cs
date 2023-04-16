using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scribble.Helpers
{
    internal static class MaterialPropertyHelper
    {
        public static readonly int PropMainTex = Shader.PropertyToID("_MainTex");
        public static readonly int PropColor = Shader.PropertyToID("_Color");

        public static void SetProperties(Material mat, Dictionary<string, string> properties)
        {
            var shader = mat.shader;
            
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                var propName = shader.GetPropertyName(i);
                if(!properties.TryGetValue(propName, out var propValue)) continue;

                var propType = shader.GetPropertyType(i);
                SetProperty(mat, propType, propName, propValue);
            }
        }

        public static void SetProperty(Material mat, ShaderPropertyType propertyType, string propertyName, string propertyValue)
        {
            switch (propertyType)
            {
                case ShaderPropertyType.Color:
                    if (ColorUtility.TryParseHtmlString(propertyValue, out Color color))
                    {
                        mat.SetColor(propertyName, color);
                    }

                    break;
                case ShaderPropertyType.Float:
                    if (float.TryParse(propertyValue, out float floatValue))
                    {
                        Debug.Log($"Setting {propertyName} to {floatValue}");
                        mat.SetFloat(propertyName, floatValue);
                    }

                    break;
                case ShaderPropertyType.Range:
                    if (float.TryParse(propertyValue, out float rangeValue))
                    {
                        mat.SetFloat(propertyName, rangeValue);
                    }

                    break;
                // case ShaderPropertyType.Vector:
                //     if (Vector4Utility.TryParse(propertyValue, out Vector4 vector))
                //     {
                //         mat.SetVector(propertyName, vector);
                //     }
                //
                //     break;
                case ShaderPropertyType.Texture:
                    // You may need to handle texture assignment based on your specific use case
                    // Example: mat.SetTexture(propertyName, texture);
                    break;
            }
        }
    }
}