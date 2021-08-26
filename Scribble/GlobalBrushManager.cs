using System;

namespace Scribble
{
    internal class GlobalBrushManager
    {
        public static BrushBehaviour LeftBrush;
        public static BrushBehaviour RightBrush;

        public static event Action<BrushBehaviour> ActiveBrushChanged;

        private static BrushBehaviour _activeBrush;

        public static BrushBehaviour ActiveBrush
        {
            get => _activeBrush;
            set
            {
                if (value == _activeBrush) return;
                _activeBrush = value;
                ActiveBrushChanged?.Invoke(value);
            }
        }

        public static bool BrushesEnabled
        {
            get
            {
                if (LeftBrush && RightBrush)
                {
                    return LeftBrush.enabled || RightBrush.enabled;
                }

                return false;
            }
            set
            {
                if (LeftBrush && RightBrush)
                {
                    LeftBrush.enabled = RightBrush.enabled = value;
                }
            }
        }
    }
}
