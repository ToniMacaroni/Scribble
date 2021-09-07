using System;
using IPA.Utilities;
using VRUIControls;
using Zenject;

namespace Scribble
{
    internal class MenuBrushManager : BaseBrushManager
    {
        private readonly MenuPlayerController _menuPlayerController;

        public MenuBrushManager(
            MenuPlayerController menuPlayerController,
            BrushBehaviour.Factory brushFactory,
            VRInputModule vrInputModule,
            ScribbleContainer container) : base(brushFactory, container)
        {
            _menuPlayerController = menuPlayerController;
            Pointer = PointerAcc(ref vrInputModule);
        }

        protected override void Init()
        {
            LeftBrush = BrushFactory.Create(_menuPlayerController.leftController.gameObject);
            LeftBrush.BrushSelected += () => { ActiveBrush = LeftBrush; };
            LeftBrush.Init(SaberType.SaberA, Pointer);

            RightBrush = BrushFactory.Create(_menuPlayerController.rightController.gameObject);
            RightBrush.BrushSelected += () => { ActiveBrush = RightBrush; };
            RightBrush.Init(SaberType.SaberB, Pointer);
        }

        private static readonly FieldAccessor<VRInputModule, VRPointer>.Accessor PointerAcc
            = FieldAccessor<VRInputModule, VRPointer>.GetAccessor("_vrPointer");
    }
}
