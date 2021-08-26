using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    public static class ParsingExtensions
    {
        public static string Delete(this string text, char[] toRemove)
        {
            foreach (var ch in toRemove)
            {
                text = text.Replace(ch.ToString(), "");
            }

            return text;
        }

        public static bool ParseVector2(string text, out Vector2 result)
        {
            result = Vector2.zero;
            string[] valueText = text.Delete(new[] {'(', ',', ')'}).Split(' ');
            float x;
            float y;
            if (!float.TryParse(valueText[0], out x) || !float.TryParse(valueText[1], out y))  return false;
            result = new Vector2(x, y);
            return true;
        }

        public static bool ParseVector3(string text, out Vector3 result)
        {
            result = Vector2.zero;
            string[] valueText = text.Delete(new[] { '(', ',', ')' }).Split(' ');
            float x;
            float y;
            float z;
            if (!float.TryParse(valueText[0], out x) || !float.TryParse(valueText[1], out y) || !float.TryParse(valueText[2], out z)) return false;
            result = new Vector3(x, y, z);
            return true;
        }
    }
}
