using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using Scribble.Views;
using UnityEngine;
using VRUIControls;
using Zenject;
using Screen = HMUI.Screen;

namespace Scribble.UI
{
    internal class RaycasterFactory : PlaceholderFactory<GameObject, VRGraphicRaycaster>{}
    internal class CustomRaycasterFactory : IFactory<GameObject, VRGraphicRaycaster>
    {
        private readonly DiContainer _container;

        public CustomRaycasterFactory(DiContainer container)
        {
            _container = container;
        }

        public VRGraphicRaycaster Create(GameObject go)
        {
            return _container.InstantiateComponent<VRGraphicRaycaster>(go);
        }
    }

    internal class ScreenFactory : PlaceholderFactory<GameObject, Screen> { }
    internal class CustomScreenFactory : IFactory<GameObject, Screen>
    {
        private readonly DiContainer _container;

        public CustomScreenFactory(DiContainer container)
        {
            _container = container;
        }

        public Screen Create(GameObject go)
        {
            return _container.InstantiateComponent<Screen>(go);
        }
    }

    internal class ViewControllerFactory : PlaceholderFactory<Type, Transform, ViewController> { }

    internal class CustomViewControllerFactory : IFactory<Type, Transform, ViewController>
    {
        private readonly DiContainer _container;

        public CustomViewControllerFactory(DiContainer container)
        {
            _container = container;
        }

        public static void Install(DiContainer container)
        {
            container
                .BindFactory<Type, Transform, ViewController, ViewControllerFactory>()
                .FromFactory<CustomViewControllerFactory>();
        }

        public ViewController Create(Type viewControllerType, Transform parent)
        {
            var canvasTemplate = Resources.FindObjectsOfTypeAll<Canvas>().First(x => x.name == "DropdownTableView");
            var gg = new GameObject(viewControllerType.Name);
            gg.AddComponent(canvasTemplate);
            _container.InstantiateComponent<VRGraphicRaycaster>(gg);
            gg.AddComponent<CanvasGroup>();
            var obj = (ViewController)_container.InstantiateComponent(viewControllerType, gg);
            obj.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
            obj.rectTransform.anchorMax = new Vector2(1f, 1f);
            obj.rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
            obj.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            obj.gameObject.SetActive(false);

            gg.transform.SetParent(parent, false);

            return obj;
        }
    }
}