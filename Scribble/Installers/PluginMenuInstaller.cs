using Scribble.UI;
using SiraUtil;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace Scribble.Installers
{
    internal class PluginMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            BrushBehaviour.Install(Container);
            Container.BindInterfacesAndSelfTo<MenuBrushManager>().AsSingle();

            Container
                .BindFactory<GameObject, VRGraphicRaycaster, RaycasterFactory>()
                .FromFactory<CustomRaycasterFactory>();

            Container
                .BindFactory<GameObject, HMUI.Screen, ScreenFactory>()
                .FromFactory<CustomScreenFactory>();

            CustomViewControllerFactory.Install(Container);

            Container.BindInterfacesAndSelfTo<ScribbleUI>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesAndSelfTo<MenuInitializer>().AsSingle();
        }
    }
}