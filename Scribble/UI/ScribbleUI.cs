using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using HMUI;
using IPA.Utilities;
using Scribble.Views;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace Scribble.UI
{
    internal class ScribbleUI : MonoBehaviour
    {
        private static DiContainer _diContainer;
        public static DiContainer DiContainer
        {
            get
            {
                if (_diContainer == null)
                {
                    var installer = FindObjectsOfType<MonoInstaller>().FirstOrDefault(i => i.GetType().Name.Contains("Menu"));
                    if (installer == null) return null;
                    _diContainer = installer.GetProperty<DiContainer, MonoInstallerBase>("Container");
                }

                return _diContainer;
            }
        }

        public static Sprite BackgroundSprite;

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

        private Material _uiMaterial;

        private bool _pressedToggleBefore;

        public static ScribbleUI Create()
        {
            GameObject go = new GameObject("ScribbleUI");
            DontDestroyOnLoad(go);
            return go.AddComponent<ScribbleUI>();
        }

        private void Awake()
        {
            gameObject.name = "ScribbleUI";
            gameObject.SetActive(false);

            BackgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Background");

            var shader = AssetLoader.LoadAsset<Shader>("assets/shaders/UIShader.shader");
            _uiMaterial = new Material(shader);
            _uiMaterial.SetColor("_Color", new Color(0.05f, 0.05f, 0.05f));
            _uiMaterial.SetColor("_GlowColor", new Color(3/255f, 152/255f, 252/255f));
            _uiMaterial.SetTexture("_Map", Tools.LoadTextureFromResources("Resources.glowmask.png"));

            transform.position = new Vector3(0, 0.1f, 0.99f);
            transform.eulerAngles = new Vector3(90f, 0f, 0f);
            transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = Camera.main;
            Debug.LogError(DiContainer != null);
            DiContainer.InstantiateComponent<VRGraphicRaycaster>(gameObject);
            _rectTransform = _canvas.transform as RectTransform;
            _rectTransform.sizeDelta = new Vector2(250f, 150f);

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.referencePixelsPerUnit = 10f;
            canvasScaler.dynamicPixelsPerUnit = 3.44f;

            var curved = gameObject.AddComponent<CurvedCanvasSettings>();
            curved.SetRadius(0);

            var screen = DiContainer.InstantiateComponent<HMUI.Screen>(gameObject);

            //TODO
            //_canvas.transform.SetParent(GameObject.Find("ScreenSystem").transform.parent);

            CreateContainer();

            LoadMainView();

            var startButton = _rectTransform.CreateSimpleButton("Start Drawing");
            startButton.SetAnchor(0.5f, 0.5f);
            startButton.SetSize(30, 10);
            startButton.SetPosition(0, -51);
            startButton.AddListener(delegate
            {
                Plugin.DrawingEnabled = !Plugin.DrawingEnabled;
                startButton.Text = Plugin.DrawingEnabled ? "Stop Drawing" : "Start Drawing";
                if (Plugin.FirstTimeLaunch && !_pressedToggleBefore)
                {
                    _pressedToggleBefore = true;
                    ScribbleContainer.Instance.LoadAnimated("second", 0.004f, true);
                }
            });

            startButton.GameObject.SetActive(true);

            CreateLogo();

            CreateToolbar();

            gameObject.SetActive(true);
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

            var sizeFitter = topToolbar.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var horizontalLayout = topToolbar.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childAlignment = TextAnchor.UpperLeft;
            horizontalLayout.spacing = 5f;

            var btn = _toolbar.CreateSimpleButton("Clear");
            btn.StrokeEnabled = false;
            btn.AddListener(delegate { if (ScribbleContainer.Instance && !ScribbleContainer.Instance.IsInAnimation) ScribbleContainer.Instance.Clear(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Undo");
            btn.StrokeEnabled = false;
            btn.AddListener(delegate { if (ScribbleContainer.Instance && !ScribbleContainer.Instance.IsInAnimation) ScribbleContainer.Instance.Undo(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Save");
            btn.StrokeEnabled = false;
            btn.AddListener(delegate { MainViewController.ShowSaveFile(); });
            btn.GameObject.SetActive(true);

            btn = _toolbar.CreateSimpleButton("Load");
            btn.StrokeEnabled = false;
            btn.AddListener(delegate { MainViewController.ShowLoadFile(); });
            btn.GameObject.SetActive(true);
        }

        private void CreateLogo()
        {
            var img = _globalContainer.CreateImage(new Vector2(20, -12), new Vector2(34, 14));
            img.rectTransform.anchorMin = new Vector2(0, 1);
            img.rectTransform.anchorMax = new Vector2(0, 1);
            img.sprite = Tools.LoadSpriteFromResources("Resources.logo.png", false);
        }

        private void LoadMainView()
        {
            MainViewController = BeatSaberUI.CreateViewController<MainViewController>();
            foreach (var curvedCanvasSettings in MainViewController.GetComponentsInChildren<CurvedCanvasSettings>())
            {
                curvedCanvasSettings.SetRadius(0);
            }
            gameObject.GetComponent<HMUI.Screen>().SetField("_rootViewController", (ViewController)MainViewController);
            MainViewController.transform.SetParent(_globalContainer.parent, false);
            MainViewController.transform.localPosition = new Vector3(0, 0, -.01f);
            MainViewController.__Activate(true, true);
        }

        public void Show(bool enable)
        {
            gameObject.SetActive(enable);
        }
    }
}
