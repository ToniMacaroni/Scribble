using System;
using System.Collections;
using BeatSaberMarkupLanguage.FloatingScreen;
using BS_Utils.Utilities;
using HMUI;
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
            set => Slider.SetField("_enableDragging", value, typeof(TextSlider));
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

        /*public bool StrokeEnabled
        {
            set => GameObject.transform.Find("Wrapper/Stroke").gameObject.SetActive(value);
        }*/

        public bool StrokeEnabled;

        public ScribbleUISimpleButton(GameObject go, string text = "button") : base(go)
        {
            go.name = text;
            TextMesh = go.GetComponentInChildren<TextMeshProUGUI>();
            go.GetComponentInChildren<LocalizedTextMeshProUGUI>().Destroy();
            NoTransitionsButton = go.GetComponent<NoTransitionsButton>();
            TextMesh.text = text;
        }

        public void AddListener(UnityAction action)
        {
            NoTransitionsButton.onClick.AddListener(action);
        }
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