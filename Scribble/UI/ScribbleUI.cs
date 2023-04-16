using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using Scribble.Views;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;
using Screen = HMUI.Screen;

namespace Scribble.UI
{
    internal class ScribbleUI : MonoBehaviour, IInitializable
    {
        public static Sprite BackgroundSprite =>
            _backgroundSprite ??= Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == "Background");
        private static Sprite _backgroundSprite;

        public MainViewController MainViewController;

        private Canvas _canvas;
        private RectTransform _rectTransform;

        private bool _shaded;
        public bool Shaded
        {
            get => _shaded;
            set
            {
                _globalContainer.parent.gameObject.SetActive(!value);
                _shaded = value;
            }
        }

        private RectTransform _globalContainer;
        private RectTransform _toolbar;
        private ScribbleUISimpleButton _startButton;
        private Material _uiMaterial;

        private bool _pressedToggleBefore;
        private AssetLoader _assetLoader;
        private RaycasterFactory _raycasterFactory;
        private ScreenFactory _screenFactory;
        private MenuBrushManager _menuBrushManager;
        private ScribbleContainer _scribbleContainer;
        private ViewControllerFactory _viewControllerFactory;
        private PluginConfig _config;
        private ScreenSystem _screenSystem;

        [Inject]
        private void Construct(
            AssetLoader assetLoader,
            RaycasterFactory raycasterFactory,
            ScreenFactory screenFactory,
            MenuBrushManager menuBrushManager,
            ScribbleContainer scribbleContainer,
            ViewControllerFactory viewControllerFactory,
            PluginConfig config,
            HierarchyManager hierarchyManager)
        {
            _assetLoader = assetLoader;
            _raycasterFactory = raycasterFactory;
            _screenFactory = screenFactory;
            _menuBrushManager = menuBrushManager;
            _scribbleContainer = scribbleContainer;
            _viewControllerFactory = viewControllerFactory;
            _config = config;
            _screenSystem = hierarchyManager.GetComponent<ScreenSystem>();
        }

        public void Initialize()
        {
            gameObject.name = "ScribbleUI";
            gameObject.SetActive(false);

            var shader = _assetLoader.LoadAsset<Shader>("assets/shaders/UIShader.shader");
            _uiMaterial = new Material(shader);
            _uiMaterial.SetColor("_Color", new Color(0.05f, 0.05f, 0.05f));
            _uiMaterial.SetColor("_GlowColor", new Color(3 / 255f, 152 / 255f, 252 / 255f));
            _uiMaterial.SetTexture("_Map", CommonHelpers.LoadTextureFromResources("Resources.glowmask.png"));

            transform.position = new Vector3(0, 0.05f, 0.99f);
            transform.eulerAngles = new Vector3(90f, 0f, 0f);
            transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;

            _raycasterFactory.Create(gameObject);

            _rectTransform = (RectTransform)_canvas.transform;
            _rectTransform.sizeDelta = new Vector2(250f, 150f);

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.referencePixelsPerUnit = 10f;
            canvasScaler.dynamicPixelsPerUnit = 3.44f;

            var curved = gameObject.AddComponent<CurvedCanvasSettings>();
            curved.SetRadius(0);

            _screenFactory.Create(gameObject);

            CreateContainer();

            LoadMainView();

            var startButtonColors = new ButtonColors
            {
                Normal = new Color(0,0,0, 0.9f),
                Highlighted = new Color(0, 0, 0, 0.6f),
                Pressed = new Color(0, 0, 0, 0.6f),
                Disabled = new Color(0,0,0, 0.2f)
            };

            _startButton = _rectTransform.CreateSimpleButton("Start Drawing", startButtonColors);
            SetupButton(_startButton);
            _startButton.SetAnchor(0.5f, 0.5f);
            _startButton.SetSize(30, 10);
            _startButton.SetPosition(0, -51);
            _startButton.AddListener(ButtonClicked);

            _startButton.GameObject.SetActive(true);

            CreateLogo();

            CreateToolbar();

            gameObject.SetActive(true);
            Shaded = true;
        }

        private void SetupButton(ScribbleUISimpleButton button)
        {
            //button.RectTransform.Find("Underline").gameObject.SetActive(false);
            //button.RectTransform.Find("BG").GetComponent<ImageView>().SetSkew(0);
        }

        private void ButtonClicked()
        {
            var drawingEnabled = _menuBrushManager.DrawingEnabled = !_menuBrushManager.DrawingEnabled;
            Shaded = !drawingEnabled;
            _startButton.Text = drawingEnabled ? "Stop Drawing" : "Start Drawing";
            _screenSystem.gameObject.SetActive(!drawingEnabled);
            if (_config.FirstTimeLaunch && !_pressedToggleBefore)
            {
                _pressedToggleBefore = true;
                _scribbleContainer.LoadAnimated("second", 0.004f, true);
            }
        }

        private void CreateContainer()
        {
            var imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(_rectTransform, false);
            var rect = imageContainer.AddComponent<RectTransform>();
            rect.anchoredPosition3D = new Vector3(0, 0, 0.01f);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);

            var img = imageContainer.AddComponent<Image>();
            img.material = _uiMaterial;
            img.type = Image.Type.Sliced;

            var container = new GameObject("Container");
            container.transform.SetParent(rect, false);
            _globalContainer = container.AddComponent<RectTransform>();
            _globalContainer.anchoredPosition3D = new Vector3(0, 0, -0.01f);
            _globalContainer.anchorMin = new Vector2(0f, 0f);
            _globalContainer.anchorMax = new Vector2(1f, 1f);
            _globalContainer.pivot = new Vector2(0.5f, 0.5f);
            _globalContainer.offsetMin = new Vector2(0, 0);
            _globalContainer.offsetMax = new Vector2(0, 0);
        }

        private void CreateToolbar()
        {
            GameObject topToolbar = new GameObject("Top_Toolbar");
            topToolbar.transform.SetParent(_globalContainer, false);
            _toolbar = topToolbar.AddComponent<RectTransform>();
            _toolbar.anchoredPosition3D = new Vector3(0, 0, -2);
            _toolbar.anchorMin = new Vector2(0f, 0.5f);
            _toolbar.anchorMax = new Vector2(1f, 0.5f);
            _toolbar.pivot = new Vector2(0.5f, 0.5f);
            _toolbar.offsetMin = new Vector2(0, 65);
            _toolbar.offsetMax = new Vector2(0, 65);
            _toolbar.localRotation = Quaternion.Euler(-30, 0, 0);
            _toolbar.sizeDelta = new Vector2(200, 10);

            var pos = _toolbar.anchoredPosition;
            pos.x = 10;
            _toolbar.anchoredPosition = pos;

            var sizeFitter = topToolbar.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var horizontalLayout = topToolbar.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childAlignment = TextAnchor.UpperLeft;
            horizontalLayout.spacing = 5f;

            var toolbarButtonColors = new ButtonColors
            {
                Normal = new Color(0, 0, 0, 1f),
                Highlighted = new Color(1, 1, 1, 0.1f),
                Pressed = new Color(1, 1, 1, 0.1f),
                Disabled = new Color(0, 0, 0, 0.2f)
            };

            var btn = _toolbar.CreateSimpleButton("Clear", toolbarButtonColors);
            SetupButton(btn);
            btn.AddListener(delegate { if (!_scribbleContainer.IsInAnimation) _scribbleContainer.Clear(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Undo", toolbarButtonColors);
            SetupButton(btn);
            btn.AddListener(delegate { if (!_scribbleContainer.IsInAnimation) _scribbleContainer.Undo(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Save", toolbarButtonColors);
            SetupButton(btn);
            btn.AddListener(delegate { MainViewController.ShowSaveFile(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Load", toolbarButtonColors);
            SetupButton(btn);
            btn.AddListener(delegate { MainViewController.ShowLoadFile(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Settings", toolbarButtonColors);
            SetupButton(btn);
            btn.AddListener(delegate { MainViewController.ShowSettings(); });
            btn.GameObject.SetActive(true);
        }

        private void CreateLogo()
        {
            var img = _globalContainer.CreateImage(new Vector2(20, -12), new Vector2(34, 14));
            img.rectTransform.anchorMin = new Vector2(0, 1);
            img.rectTransform.anchorMax = new Vector2(0, 1);
            img.sprite = CommonHelpers.LoadSpriteFromResources("Resources.logo.png", false);
        }

        private void LoadMainView()
        {
            MainViewController = (MainViewController)_viewControllerFactory.Create(typeof(MainViewController), _globalContainer.parent);
            foreach (var curvedCanvasSettings in MainViewController.GetComponentsInChildren<CurvedCanvasSettings>())
            {
                curvedCanvasSettings.SetRadius(0);
            }
            gameObject.GetComponent<Screen>().SetField("_rootViewController", (ViewController)MainViewController);
            MainViewController.transform.localPosition = new Vector3(0, 0, -.01f);
            MainViewController.__Activate(true, true);
        }

        public void Show(bool enable)
        {
            gameObject.SetActive(enable);
        }
    }
}
