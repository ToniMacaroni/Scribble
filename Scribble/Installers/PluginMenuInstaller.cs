using UnityEngine;
using Zenject;

namespace Scribble.Installers
{
    internal class PluginMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ControllerGetter>().AsSingle();
        }
    }

    internal class ControllerGetter : IInitializable
    {
        private readonly MenuPlayerController _menuPlayerController;

        public ControllerGetter(MenuPlayerController menuPlayerController)
        {
            _menuPlayerController = menuPlayerController;
        }

        public void Initialize()
        {
            Debug.LogError("Found");
            Debug.LogError(_menuPlayerController != null);
        }
    }
}