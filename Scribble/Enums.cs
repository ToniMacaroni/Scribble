using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribble
{
    public enum SaberHand {
        Right,
        Left
    }

    public static class EnumExtensions
    {
        public static TEnum GetEnumValue<TEnum>(this string name)
        {
            return (TEnum) Enum.Parse(typeof(TEnum), name);
        }
    }
}
