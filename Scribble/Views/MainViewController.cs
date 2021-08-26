using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using Scribble.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRUIControls;

namespace Scribble.Views
{
    internal class MainViewController : BSMLResourceViewController
    {
        public static Color ListCellColor = new Color(0.3f, 0.3f, 0.3f, 0.15f);

        [UIComponent("brushList")]
        private CustomCellListTableData _brushList;

        [UIValue("brushes")] private List<object> _brushes = new List<object>();

        [UIComponent("textureList")]
        private CustomCellListTableData _texureList;

        [UIValue("textures")] private List<object> _textures = new List<object>();

        [UIComponent("effectsList")]
        private CustomCellListTableData _effectsList;

        [UIValue("effects")] private List<object> _effects = new List<object>();

        [UIComponent("SizeSlider")] private GenericSliderSetting _sizeSlider;
        [UIComponent("GlowSlider")] private GenericSliderSetting _glowSlider;

        [UIValue("Glow")]
        public float Glow
        {
            get
            {
                if (GlobalBrushManager.ActiveBrush) return GlobalBrushManager.ActiveBrush.CurrentBrush.Glow;
                return 0.5f;
            }
            set
            {
                if (GlobalBrushManager.ActiveBrush) GlobalBrushManager.ActiveBrush.CurrentBrush.Glow = value;
            }
        }

        [UIValue("Size")]
        public float Size
        {
            get
            {
                if (GlobalBrushManager.ActiveBrush) return GlobalBrushManager.ActiveBrush.CurrentBrush.Size;
                return 1f;
            }
            set
            {
                if (GlobalBrushManager.ActiveBrush) GlobalBrushManager.ActiveBrush.CurrentBrush.Size = value;
            }
        }

        [UIValue("brush-color-value")] private Color _brushColorValue = Color.white;

        [UIComponent("eraser-btn")] private Transform _eraserBtn;
        [UIComponent("picker-btn")] private Transform _pickerBtn;
        [UIComponent("color-picker-modal")] public ModalView ColorPickerModal;


        #region Saving and Loading

        [UIComponent("save-dialog")] private ModalView SaveModal;
        [UIComponent("load-dialog")] private ModalView LoadModal;

        [UIComponent("save-file-list")] private CustomListTableData SaveFileList;
        [UIComponent("load-file-list")] private CustomListTableData LoadFileList;

        readonly List<CustomListTableData.CustomCellInfo> FileList = new List<CustomListTableData.CustomCellInfo>();
        readonly List<string> FilePaths = new List<string>();

        [UIComponent("new-file-string")] private StringSetting _newFileString;
        [UIValue("save-file-name")] private string _saveFilename = "Drawing";

        private ImageView _erazerImage;

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
            if (!ScribbleContainer.Instance) return;
            if (string.IsNullOrEmpty(_saveFilename)) return;
            FileInfo file = new FileInfo(Path.Combine(SaveSystem.SaveDirectory.FullName, _saveFilename + ".png"));
            ScribbleContainer.Instance.Save(file);
            SaveModal.Hide(true);
        }

        [UIAction("file-save-selected")]
        private void SaveSelectedFile(TableView _, int row)
        {
            if (!ScribbleContainer.Instance) return;
            ScribbleContainer.Instance.Save(new FileInfo(FilePaths[row]));
            SaveModal.Hide(true);
        }

        [UIAction("file-load-selected")]
        private void LoadSelectedFile(TableView _, int row)
        {
            if (!ScribbleContainer.Instance) return;
            ScribbleContainer.Instance.Load(new FileInfo(FilePaths[row]));
        }

        IEnumerator CRShowLoadingModal(ModalView modal, CustomListTableData customListTableData)
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
            StartCoroutine(CRShowLoadingModal(LoadModal, LoadFileList));
        }

        public void ShowSaveFile()
        {
            StartCoroutine(CRShowLoadingModal(SaveModal, SaveFileList));
        }

        private void UpdateFileList()
        {
            FileList.Clear();
            FilePaths.Clear();
            if (!SaveSystem.SaveDirectory.Exists)
            {
                SaveSystem.SaveDirectory.Create();
                SaveSystem.SaveDirectory.Refresh();
                return;
            }
            foreach (var saveFile in SaveSystem.SaveDirectory.GetFiles("*.png"))
            {
                Texture2D tex = Tools.LoadTextureFromFile(saveFile.FullName, false);
                CustomListTableData.CustomCellInfo cellInfo = new CustomListTableData.CustomCellInfo(saveFile.FilenameWithoutExtension(), null, tex.ToSprite());
                FileList.Add(cellInfo);
                FilePaths.Add(saveFile.FullName);
            }
        }

        #endregion


        [UIAction("brush-selected")]
        public void SelectBrush(TableView _, UIBrushObject brush)
        {
            GlobalBrushManager.ActiveBrush.EraseMode = false;
            GlobalBrushManager.ActiveBrush.CurrentBrush = brush.Brush;
            SelectForBrush(brush.Brush);
        }

        [UIAction("texture-selected")]
        public void SelectTexture(TableView _, UITextureObject texture)
        {
            GlobalBrushManager.ActiveBrush.CurrentBrush.TextureName = texture.Name;
        }

        [UIAction("effect-selected")]
        public void SelectEffect(TableView _, UIEffectObjects effect)
        {
            GlobalBrushManager.ActiveBrush.CurrentBrush.EffectName = effect.Name;
        }

        [UIAction("selectEraseMode")]
        public void SelectEraseMode()
        {
            if (GlobalBrushManager.ActiveBrush)
            {
                var on = !GlobalBrushManager.ActiveBrush.EraseMode;
                GlobalBrushManager.ActiveBrush.EraseMode = on;
                _erazerImage.color = on ? Color.red : Color.white;
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
            if (!GlobalBrushManager.ActiveBrush) return;
            GlobalBrushManager.ActiveBrush.CurrentBrush.ColorString = "#"+ ColorUtility.ToHtmlStringRGBA(color);
            (_brushList.data[GlobalBrushManager.ActiveBrush.CurrentBrush.Index] as UIBrushObject)?.ManualRefresh();
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            SetupBrushes();
            SetupTextures();
            SetupEffects();

            //Loading texture without mipchain
            _erazerImage = _eraserBtn.gameObject.GetComponent<ImageView>("Content/Icon");
            var img2 = _pickerBtn.gameObject.GetComponent<ImageView>("Content/Icon");
            _erazerImage.sprite = Tools.LoadSpriteFromResources("Icons.eraser.png", false);
            _erazerImage.SetField("_skew", 0f);
            img2.sprite = Tools.LoadSpriteFromResources("Icons.picker.png", false);
            img2.SetField("_skew", 0f);

            SaveFileList.data = FileList;
            LoadFileList.data = FileList;

            SetModalPosition(ColorPickerModal);
            SetModalPosition(_newFileString.modalKeyboard.modalView);
            SetModalPosition(LoadModal);
            SetModalPosition(SaveModal);

            FixTextEdit();

            GlobalBrushManager.ActiveBrushChanged += ActiveControllerChanged;
            ActiveControllerChanged(GlobalBrushManager.ActiveBrush);
        }

        public void SetupBrushes()
        {
            _brushList.data.Clear();
            foreach (var brush in Brushes.BrusheList)
            {
                _brushes.Add(new UIBrushObject(brush));
            }
            _brushList.tableView.ReloadData();
        }

        public void SetupTextures()
        {
            _texureList.data.Clear();
            foreach (var texture in BrushTextures.Textures)
            {
                _textures.Add(new UITextureObject(texture));
            }
            _texureList.tableView.ReloadData();
        }

        public void SetupEffects()
        {
            _effectsList.data.Clear();
            foreach (var effect in Effects.EffectsList)
            {
                _effects.Add(new UIEffectObjects(effect));
            }
            _effectsList.tableView.ReloadData();
        }

        public void ActiveControllerChanged(BrushBehaviour brush)
        {

            int selectedBrush = Brushes.BrusheList.FindIndex(br => br.Name == brush.CurrentBrush.Name);
            if (selectedBrush!=-1)
            {
                _brushList.tableView.ScrollToCellWithIdx(selectedBrush, TableView.ScrollPositionType.Beginning, false);
                _brushList.tableView.SelectCellWithIdx(selectedBrush);
            }

            SelectForBrush(Brushes.BrusheList[selectedBrush]);
        }

        public void SelectForBrush(CustomBrush brush)
        {
            int selectedTexture = BrushTextures.Textures.FindIndex(tex => tex.Name == brush.TextureName);
            if (selectedTexture != -1)
            {
                _texureList.tableView.ScrollToCellWithIdx(selectedTexture, TableView.ScrollPositionType.Beginning, false);
                _texureList.tableView.SelectCellWithIdx(selectedTexture);
            }

            int selectedEffect = Effects.EffectsList.FindIndex(fx => fx.Name == brush.EffectName);
            if (selectedEffect != -1)
            {
                _effectsList.tableView.ScrollToCellWithIdx(selectedEffect, TableView.ScrollPositionType.Beginning, false);
                _effectsList.tableView.SelectCellWithIdx(selectedEffect);
            }

            _sizeSlider.ReceiveValue();
            _glowSlider.ReceiveValue();

            _brushColorValue = brush.Color;
        }

        private void SetModalPosition(ModalView modal)
        {
            modal.transform.localPosition = new Vector3(0, 0, -20);
            modal.transform.localRotation = Quaternion.Euler(-20, 0, 0);
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

        public UIEffectObjects(Effect effect)
        {
            Effect = effect;
            Name = effect.Name;

            effectMaterial = effect.CreateMaterial(Brushes.EffectBrush);
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
