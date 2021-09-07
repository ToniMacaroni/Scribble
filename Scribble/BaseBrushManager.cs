using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scribble.Tools;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace Scribble
{
    internal abstract class BaseBrushManager : IInitializable
    {
        public BrushBehaviour LeftBrush;
        public BrushBehaviour RightBrush;

        public event Action<BrushBehaviour> ActiveBrushChanged;

        private BrushBehaviour _activeBrush;

        public BrushBehaviour ActiveBrush
        {
            get => _activeBrush;
            set
            {
                if (value == _activeBrush) return;
                _activeBrush = value;
                ActiveBrushChanged?.Invoke(value);
            }
        }

        public bool BrushesEnabled
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

        public bool DrawingEnabled
        {
            get => Container.DrawingEnabled;
            set
            {
                Container.DrawingEnabled = value;
                BrushesEnabled = value;
            }
        }

        protected readonly BrushBehaviour.Factory BrushFactory;
        protected readonly ScribbleContainer Container;
        protected VRPointer Pointer;

        protected BaseBrushManager(BrushBehaviour.Factory brushFactory, ScribbleContainer container)
        {
            BrushFactory = brushFactory;
            Container = container;
        }

        public void Initialize()
        {
            Init();
            DrawingEnabled = false;
        }

        public void SetTool<T>()
        {
            ActiveBrush?.SetTool(typeof(T).Name);
        }

        public void SetBrush(CustomBrush customBrush)
        {
            if (ActiveBrush is { }) ActiveBrush.CurrentBrush = customBrush;
        }

        public CustomBrush GetCurrentBrush()
        {
            if (ActiveBrush is null) return null;
            return ActiveBrush.CurrentBrush;
        }

        protected abstract void Init();
    }
}