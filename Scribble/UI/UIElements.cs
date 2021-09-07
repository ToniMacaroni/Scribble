using System;
using System.Collections;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using IPA.Utilities;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Scribble
{
    public class ScribbleUIElement
    {
        public GameObject GameObject;
        public RectTransform RectTransform;

        public ScribbleUIElement(GameObject go)
        {
            GameObject = go;
            RectTransform = go.transform as RectTransform;
        }

        public void SetAnchor(float x, float y)
        {
            SetAnchor(x, y, x, y);
        }

        public void SetAnchor(float minX, float minY, float maxX, float maxY)
        {
            RectTransform.anchorMin = new Vector2(minX, minY);
            RectTransform.anchorMax = new Vector2(maxX, maxY);
        }

        public void SetPosition(float x, float y)
        {
            RectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void SetSize(float x, float y)
        {
            RectTransform.sizeDelta = new Vector2(x, y);
        }
    }

    public class ScribbleUISlider : ScribbleUIElement
    {
        public RangeValuesTextSlider Slider;
        public TextMeshProUGUI TextMesh;

        public bool EnableDragging
        {
            set => Slider.SetField<TextSlider, bool>("_enableDragging", value);
        }

        public string Text
        {
            set => TextMesh.text = value;
        }

        public ScribbleUISlider(GameObject go) : base(go)
        {
            Slider = go.GetComponentInChildren<RangeValuesTextSlider>();
            TextMesh = go.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetRange(float min, float max)
        {
            Slider.minValue = min;
            Slider.maxValue = max;
        }
    }

    public class ScribbleUISimpleButton : ScribbleUIElement
    {
        public NoTransitionsButton NoTransitionsButton;
        public TextMeshProUGUI TextMesh;

        public string Text
        {
            set => TextMesh.text = value;
        }

        public ScribbleUISimpleButton(GameObject go, string text = "button", ButtonColors? buttonColors = null) : base(go)
        {
            go.name = text;
            TextMesh = go.GetComponentInChildren<TextMeshProUGUI>();
            go.GetComponentInChildren<LocalizedTextMeshProUGUI>().Destroy();
            NoTransitionsButton = go.GetComponent<NoTransitionsButton>();

            if (buttonColors.HasValue)
            {
                go.AddComponent<ButtonColorManager>().Init(buttonColors.Value);
            }

            TextMesh.text = text;
        }

        

        public void AddListener(UnityAction action)
        {
            NoTransitionsButton.onClick.AddListener(action);
        }
    }

    internal class ButtonColorManager : MonoBehaviour
    {
        private ButtonColors _colors;
        private ImageView _bg;

        public void Init(ButtonColors colors)
        {
            _colors = colors;
            _bg = transform.Find("BG").GetComponent<ImageView>();
            var noTransitionButton = GetComponent<NoTransitionsButton>();
            var anim = GetComponent<ButtonStaticAnimations>();
            noTransitionButton.selectionStateDidChangeEvent -= anim.HandleButtonSelectionStateDidChange;
            noTransitionButton.selectionStateDidChangeEvent += OnStateChange;
            Destroy(anim);
        }

        private void OnStateChange(NoTransitionsButton.SelectionState state)
        {
            switch (state)
            {
                case NoTransitionsButton.SelectionState.Normal:
                    _bg.color = _colors.Normal;
                    break;
                case NoTransitionsButton.SelectionState.Highlighted:
                    _bg.color = _colors.Highlighted;
                    break;
                case NoTransitionsButton.SelectionState.Pressed:
                    _bg.color = _colors.Pressed;
                    break;
                case NoTransitionsButton.SelectionState.Disabled:
                    _bg.color = _colors.Disabled;
                    break;
            }
        }
    }

    public struct ButtonColors
    {
        public Color Normal;
        public Color Highlighted;
        public Color Pressed;
        public Color Disabled;
    }

    public class ScribbleUICheckbox : ScribbleUIElement
    {
        public Toggle Toggle;
        public TextMeshProUGUI TextMesh;

        public string Text
        {
            set => TextMesh.text = value;
        }

        public ScribbleUICheckbox(GameObject go) : base(go)
        {
            Toggle = go.GetComponentInChildren<Toggle>();
            TextMesh = go.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void AddListener(UnityAction<bool> action)
        {
            Toggle.onValueChanged.AddListener(action);
        }
    }
}