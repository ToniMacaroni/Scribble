using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.ViewControllers;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using Scribble.Effects;
using Scribble.Stores;
using Scribble.Tools;
using Scribble.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Scribble.Views
{
    internal class MainViewController : BSMLResourceViewController
    {
        public static readonly Color ListCellColor = new Color(1f, 1f, 1f, 0.15f);

        [UIParams] private readonly BSMLParserParams _params = null;

        [UIComponent("brushList")] private readonly CustomCellListTableData _brushList = null;

        [UIValue("brushes")] protected readonly List<object> Brushes = new List<object>();

        [UIComponent("textureList")] private readonly CustomCellListTableData _texureList = null;

        [UIValue("textures")] protected readonly List<object> Textures = new List<object>();

        [UIComponent("effectsList")] private readonly CustomCellListTableData _effectsList = null;

        [UIValue("effects")] protected readonly List<object> Effects = new List<object>();

        [UIComponent("SizeSlider")] private readonly GenericSliderSetting _sizeSlider = null;
        [UIComponent("GlowSlider")] private readonly GenericSliderSetting _glowSlider = null;

        [UIValue("brush-color-value")] private Color _brushColorValue = Color.white;

        [UIComponent("eraser-btn")] private readonly Transform _eraserBtn = null;
        [UIComponent("picker-btn")] private readonly Transform _pickerBtn = null;
        [UIComponent("move-btn")] private readonly Transform _moveBtn = null;
        [UIComponent("scale-btn")] private readonly Transform _scaleBtn = null;
        [UIComponent("line-width-btn")] private readonly Transform _lineWidthBtn = null;
        [UIComponent("bucket-btn")] private readonly Transform _bucketBtn = null;
        [UIComponent("color-picker-modal")] public ModalView ColorPickerModal;

        [UIObject("stop-animation-button")] private readonly GameObject _stopAnimButton = null;

        [Inject]
        private void Construct(
            MenuBrushManager menuBrushManager,
            ScribbleContainer scribbleContainer,
            SaveSystem saveSystem,
            BrushStore brushStore,
            EffectStore effects,
            BrushTextures textures,
            PluginConfig pluginConfig)
        {
            _brushManager = menuBrushManager;
            _scribbleContainer = scribbleContainer;
            _saveSystem = saveSystem;
            _brushStoreStore = brushStore;
            _effectStore = effects;
            _textureStore = textures;
            _pluginConfig = pluginConfig;
        }

        public float Glow
        {
            get => _brushManager.ActiveBrush?.CurrentBrush?.Glow??0.5f;
            set
            {
                if (_brushManager.ActiveBrush?.CurrentBrush is {} brush) brush.Glow = value;
            }
        }

        public float Size
        {
            get => _brushManager.ActiveBrush?.CurrentBrush?.Size ?? 1f;
            set
            {
                if (_brushManager.ActiveBrush?.CurrentBrush is {} brush) brush.Size = value;
            }
        }

        public bool ShowInMap
        {
            get => _pluginConfig.VisibleDuringPlay;
            set
            {
                _pluginConfig.VisibleDuringPlay = value;
                _scribbleContainer.SetContext(value);
            }
        }

        public bool LoadDrawingsAnimated
        {
            get => _pluginConfig.LoadDrawingsAnimated;
            set => _pluginConfig.LoadDrawingsAnimated = value;
        }

        private ImageView _currentlySelectedToolIcon;
        private Dictionary<Type, ImageView> _toolIcons;
        private MenuBrushManager _brushManager;
        private ScribbleContainer _scribbleContainer;
        private SaveSystem _saveSystem;
        private BrushStore _brushStoreStore;
        private EffectStore _effectStore;
        private BrushTextures _textureStore;
        private PluginConfig _pluginConfig;

        private ButtonColors _buttonColors;

        #region Saving and Loading

        [UIComponent("save-dialog")] protected readonly ModalView SaveModal = null;
        [UIComponent("load-dialog")] protected readonly ModalView LoadModal = null;
        [UIComponent("settings-modal")] protected readonly ModalView SettingsModal = null;

        [UIComponent("save-file-list")] protected CustomListTableData SaveFileList;
        [UIComponent("load-file-list")] protected CustomListTableData LoadFileList;

        protected readonly List<CustomListTableData.CustomCellInfo> FileList = new List<CustomListTableData.CustomCellInfo>();
        protected readonly List<string> FilePaths = new List<string>();

        [UIComponent("new-file-string")] protected readonly StringSetting _newFileString = null;
        [UIValue("save-file-name")] private string _saveFilename = "Drawing";

        [UIAction("save-file-name-changed")]
        public void SaveFilenameChanged(string text)
        {
            FixTextEdit();
        }

        private void FixTextEdit()
        {
            //_newFileString.text.fontSize = 5f;
            _newFileString.text.rectTransform.anchorMin = new Vector2(0, 0.5f);
            _newFileString.text.rectTransform.anchorMax = new Vector2(0.6f, 0.5f);
            _newFileString.text.rectTransform.sizeDelta = new Vector2(7, 30);
            _newFileString.text.rectTransform.anchoredPosition = new Vector2(-4, 0);
        }

        [UIAction("save-new")]
        private void SaveNewFile()
        {
            if (string.IsNullOrEmpty(_saveFilename)) return;
            FileInfo file = new FileInfo(Path.Combine(_saveSystem.SaveDirectory.FullName, _saveFilename + ".png"));
            _scribbleContainer.Save(file);
            SaveModal.Hide(true);
        }

        [UIAction("file-save-selected")]
        private void SaveSelectedFile(TableView tableView, int row)
        {
            if (!_scribbleContainer) return;
            _scribbleContainer.Save(new FileInfo(FilePaths[row]));
            tableView.ClearSelection();
            SaveModal.Hide(true);
        }

        [UIAction("file-load-selected")]
        private void LoadSelectedFile(TableView tableView, int row)
        {
            if (!_scribbleContainer) return;
            _scribbleContainer.Load(new FileInfo(FilePaths[row]));
            tableView.ClearSelection();
            LoadModal.Hide(true);
        }

        IEnumerator CrShowLoadingModal(ModalView modal, CustomListTableData customListTableData)
        {
            UpdateFileList();
            customListTableData.tableView.ReloadData();
            modal.Show(false);
            SetSkewForChildren(customListTableData.tableView.gameObject, 0);
            yield return new WaitForSeconds(0.1f);
            customListTableData.tableView.GetField<ScrollView, TableView>("_scrollView")._fixedCellSize = 23f;
        }

        public void ShowLoadFile()
        {
            StartCoroutine(CrShowLoadingModal(LoadModal, LoadFileList));
        }

        public void ShowSaveFile()
        {
            StartCoroutine(CrShowLoadingModal(SaveModal, SaveFileList));
        }

        private void UpdateFileList()
        {
            FileList.Clear();
            FilePaths.Clear();
            foreach (var saveFile in _saveSystem.SaveDirectory.GetFiles("*.png"))
            {
                Texture2D tex = CommonHelpers.LoadTextureFromFile(saveFile.FullName, false);
                CustomListTableData.CustomCellInfo cellInfo = new CustomListTableData.CustomCellInfo(saveFile.FilenameWithoutExtension(), null, tex.ToSprite());
                FileList.Add(cellInfo);
                FilePaths.Add(saveFile.FullName);
            }
        }

        #endregion


        [UIAction("brush-selected")]
        public void SelectBrush(TableView _, UIBrushObject brush)
        {
            _brushManager.SetBrush(brush.Brush);
            _brushManager.SetTool<DrawingTool>();
            _toolIcons.Values.Do(x=>x.color = Color.white);
            _brushManager.ActiveBrush?.SelectedTool?.OnSelected();
            SelectForBrush(brush.Brush);
        }

        [UIAction("texture-selected")]
        public void SelectTexture(TableView _, UITextureObject texture)
        {
            _brushManager.GetCurrentBrush().TextureName = texture.Name;
        }

        [UIAction("effect-selected")]
        public void SelectEffect(TableView _, UIEffectObjects effect)
        {
            _brushManager.GetCurrentBrush().EffectName = effect.Name;
        }

        public void SelectTool(Type toolType)
        {
            if (!_brushManager.ActiveBrush) return;
            if (_brushManager.ActiveBrush.SelectedTool.GetType() == toolType)
            {
                _brushManager.SetTool<DrawingTool>();
            }
            else
            {
                _brushManager.ActiveBrush.SetTool(toolType.Name);
            }

            if(_currentlySelectedToolIcon is {}) _currentlySelectedToolIcon.color = Color.white;

            if (_toolIcons.TryGetValue(_brushManager.ActiveBrush.SelectedTool.GetType(), out var newIcon))
            {
                _currentlySelectedToolIcon = newIcon;
                newIcon.color = Color.red;
            }
        }

        [UIAction("selectPicker")]
        public void SelectPicker()
        {
            ColorPickerModal.Show(true);
        }

        [UIAction("picker-selected-color")]
        public void PickerSelectedColor(Color color)
        {
            if (!_brushManager.ActiveBrush) return;
            var brush = _brushManager.GetCurrentBrush();
            brush.ColorString = "#" + ColorUtility.ToHtmlStringRGBA(color);
            (_brushList.data[brush.Index] as UIBrushObject)?.ManualRefresh();
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (_scribbleContainer is {})
            {
                _scribbleContainer.OnAnimationStateChanged += ScribbleContainerOnOnAnimationStarted;
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (_scribbleContainer is {})
            {
                _scribbleContainer.OnAnimationStateChanged -= ScribbleContainerOnOnAnimationStarted;
            }
        }

        private void ScribbleContainerOnOnAnimationStarted(bool isAnimating)
        {
            _stopAnimButton.SetActive(isAnimating);
        }

        [UIAction("stop-animation")]
        private void StopAnimation()
        {
            if (_scribbleContainer is { })
            {
                _scribbleContainer.StopAnimation = true;
            }
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            _buttonColors = new ButtonColors
            {
                Normal = new Color(0, 0, 0, 1),
                Highlighted = new Color(1, 1, 1, 0.2f),
                Pressed = new Color(1, 1, 1, 0.2f),
                Disabled = new Color(0, 0, 0, 0.6f)
            };

            _stopAnimButton.SetActive(false);

            SetupBrushes();
            SetupTextures();
            SetupEffects();

            _toolIcons = new Dictionary<Type, ImageView>();

            SetupTool(_eraserBtn, "Icons.eraser.png", typeof(Eraser));
            SetupTool(_moveBtn, "Icons.move.png", typeof(MoveTool));
            SetupTool(_scaleBtn, "Icons.resize.png", typeof(ScaleTool));
            SetupTool(_lineWidthBtn, "Icons.line-width.png", typeof(LineWidthTool));
            SetupTool(_bucketBtn, "Icons.bucket.png", typeof(BucketTool));
            SetupIconButton(_pickerBtn, "Icons.picker.png", out _);

            foreach (var buttonGo in _params.GetObjectsWithTag("custom-button"))
            {
                SetupButton(buttonGo.transform);
            }

            foreach (var modalGo in _params.GetObjectsWithTag("custom-modal"))
            {
                SetModalPosition(modalGo.transform);
            }

            SaveFileList.data = FileList;
            LoadFileList.data = FileList;

            SetModalPosition(_newFileString.modalKeyboard.modalView.transform);

            FixTextEdit();

            _brushManager.ActiveBrushChanged += ActiveControllerChanged;
        }

        private void SetupTool(Transform button, string imageName, Type toolType)
        {
            SetupIconButton(button, imageName, out var icon);
            _toolIcons.Add(toolType, icon);
            button.GetComponent<NoTransitionsButton>().onClick.AddListener(() =>
            {
                SelectTool(toolType);
            });
        }

        private void SetupIconButton(Transform button, string imageName, out ImageView iconImage)
        {
            iconImage = button.gameObject.GetComponent<ImageView>("Content/Icon");
            iconImage.sprite = CommonHelpers.LoadSpriteFromResources(imageName, false);
            iconImage.SetSkew(0);

            SetupButton(button);
        }

        private void SetupButton(Transform button)
        {
            button.gameObject.GetComponent<ImageView>("BG").SetSkew(0);
            button.gameObject.GetComponent<ImageView>("Underline").gameObject.SetActive(false);
            button.gameObject.AddComponent<ButtonColorManager>().Init(_buttonColors);
        }

        public void ShowSettings()
        {
            SettingsModal.Show(false);
        }

        public void SetupBrushes()
        {
            _brushList.data.Clear();
            foreach (var brush in _brushStoreStore.BrushList)
            {
                Brushes.Add(new UIBrushObject(brush));
            }
            _brushList.tableView.ReloadData();
        }

        public void SetupTextures()
        {
            _texureList.data.Clear();
            foreach (var texture in _textureStore.Textures)
            {
                Textures.Add(new UITextureObject(texture));
            }
            _texureList.tableView.ReloadData();
        }

        public void SetupEffects()
        {
            _effectsList.data.Clear();
            foreach (var effect in _effectStore.EffectsList)
            {
                Effects.Add(new UIEffectObjects(effect, _brushStoreStore));
            }
            _effectsList.tableView.ReloadData();
        }

        public void ActiveControllerChanged(BrushBehaviour brush)
        {
            int selectedBrush = _brushStoreStore.BrushList.FindIndex(br => br.Name == brush.CurrentBrush.Name);
            if (selectedBrush != -1)
            {
                _brushList.tableView.ScrollToCellWithIdx(selectedBrush, TableView.ScrollPositionType.Beginning, false);
                _brushList.tableView.SelectCellWithIdx(selectedBrush);
            }

            SelectForBrush(_brushStoreStore.BrushList[selectedBrush]);
        }

        public void SelectForBrush(CustomBrush brush)
        {
            int selectedTexture = _textureStore.Textures.FindIndex(tex => tex.Name == brush.TextureName);
            if (selectedTexture != -1)
            {
                _texureList.tableView.ScrollToCellWithIdx(selectedTexture, TableView.ScrollPositionType.Beginning, false);
                _texureList.tableView.SelectCellWithIdx(selectedTexture);
            }

            int selectedEffect = _effectStore.EffectsList.FindIndex(fx => fx.Name == brush.EffectName);
            if (selectedEffect != -1)
            {
                _effectsList.tableView.ScrollToCellWithIdx(selectedEffect, TableView.ScrollPositionType.Beginning, false);
                _effectsList.tableView.SelectCellWithIdx(selectedEffect);
            }

            _sizeSlider.ReceiveValue();
            _glowSlider.ReceiveValue();

            _brushColorValue = brush.Color;
        }

        private void SetModalPosition(Transform modal)
        {
            modal.localPosition = new Vector3(0, 0, -20);
            modal.localRotation = Quaternion.Euler(-20, 0, 0);
        }

        private void SetSkewForChildren(GameObject root, float skew)
        {
            foreach (var imageView in root.GetComponentsInChildren<ImageView>())
            {
                imageView.SetField("_skew", (float)skew);
            }
        }

        public override string ResourceName { get; } = "Scribble.Views.MainView.bsml";
    }

    internal class UIBrushObject
    {
        public CustomBrush Brush;

        [UIValue("Name")] public string Name => Brush.Name;

        [UIValue("HexColor")] public string HexColor => Brush.ColorString;

        [UIComponent("HexColorText")] private TextMeshProUGUI HexColorText;

        [UIComponent("ColorImage")]
        public RawImage ColorImage;

        [UIComponent("HoveredBackground")]
        public RawImage HoveredBackground;

        [UIComponent("SelectedBackground")]
        public RawImage SelectedBackground;

        public UIBrushObject(CustomBrush brush)
        {
            Brush = brush;
        }

        [UIAction("refresh-visuals")]
        public void Refresh(bool selected, bool highlighted)
        {
            HoveredBackground.texture = null;
            HoveredBackground.color = MainViewController.ListCellColor;

            SelectedBackground.texture = null;
            SelectedBackground.color = MainViewController.ListCellColor;

            ColorImage.texture = ScribbleUI.BackgroundSprite.texture;
            ColorImage.color = Brush.Color;
        }

        public void ManualRefresh()
        {
            ColorImage.color = Brush.Color;
            HexColorText.text = HexColor;
        }
    }

    internal class UITextureObject
    {
        public BrushTextures.BrushTexture Texture;

        [UIValue("Name")] public string Name;

        [UIComponent("TextureImage")]
        public RawImage TextureImage;

        [UIComponent("HoveredBackground")]
        public RawImage HoveredBackground;

        [UIComponent("SelectedBackground")]
        public RawImage SelectedBackground;

        public UITextureObject(BrushTextures.BrushTexture texture)
        {
            Texture = texture;
            Name = texture.Name;
        }

        [UIAction("refresh-visuals")]
        public void Refresh(bool selected, bool highlighted)
        {
            HoveredBackground.texture = null;
            HoveredBackground.color = MainViewController.ListCellColor;

            SelectedBackground.texture = null;
            SelectedBackground.color = MainViewController.ListCellColor;

            TextureImage.texture = Texture.Texture;
        }
    }

    internal class UIEffectObjects
    {
        public Effect Effect;

        [UIValue("Name")] public string Name;

        [UIComponent("EffectImage")]
        public RawImage EffectImage;

        [UIComponent("HoveredBackground")]
        public RawImage HoveredBackground;

        [UIComponent("SelectedBackground")]
        public RawImage SelectedBackground;

        private Material effectMaterial;

        public UIEffectObjects(Effect effect, BrushStore brushStore)
        {
            Effect = effect;
            Name = effect.Name;

            effectMaterial = effect.CreateMaterial(brushStore.EffectBrush);
        }

        [UIAction("refresh-visuals")]
        public void Refresh(bool selected, bool highlighted)
        {
            HoveredBackground.texture = null;
            HoveredBackground.color = MainViewController.ListCellColor;

            SelectedBackground.texture = null;
            SelectedBackground.color = MainViewController.ListCellColor;

            EffectImage.material = effectMaterial;
        }
    }
}
