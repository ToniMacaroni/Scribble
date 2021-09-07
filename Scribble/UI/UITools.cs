using System.Linq;
using HMUI;
using IPA.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;

namespace Scribble
{
    internal static class UITools
    {
        public static TextMeshProUGUI CreateText(this RectTransform parent, string text, Vector2 anchoredPosition)
        {
            var textGo = new GameObject("Text");
            textGo.SetActive(false);
            var textComp = textGo.AddComponent<TextMeshProUGUI>();
            textComp.font = Object.Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .First(t => t.name == "Teko-Medium SDF No Glow"));
            textComp.rectTransform.SetParent(parent, false);
            textComp.text = text;
            textComp.fontSize = 14f;
            textComp.overrideColorTags = true;
            textComp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textComp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textComp.rectTransform.sizeDelta = new Vector2(30f, 15f);
            textComp.rectTransform.anchoredPosition = anchoredPosition;
            textComp.rectTransform.pivot = new Vector2(0, 0.5f);

            var fitter = textGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            textGo.SetActive(true);

            return textComp;
        }

        public static Image CreateImage(this RectTransform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var image_go = new GameObject("Image");
            var imageComp = image_go.AddComponent<Image>();
            imageComp.rectTransform.SetParent(parent, false);
            imageComp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            imageComp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            imageComp.rectTransform.sizeDelta = sizeDelta;
            imageComp.rectTransform.anchoredPosition = anchoredPosition;

            return imageComp;
        }

        public static GameObject CreateFromInstance(this Transform parent, string name, bool worldPositionStays = false)
        {
            GameObject go = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == name);
            return Object.Instantiate(go, parent, worldPositionStays);
        }

        public static ScribbleUISlider CreateSlider(this Transform parent)
        {
            GameObject go = parent.CreateFromInstance("PositionX");
            return new ScribbleUISlider(go);
        }

        public static ScribbleUISimpleButton CreateSimpleButton(this Transform parent, string text, ButtonColors? buttonColors = null)
        {
            GameObject go = parent.CreateFromInstance("ApplyButton");
            return new ScribbleUISimpleButton(go, text, buttonColors);
        }

        public static ScribbleUICheckbox CreateCheckbox(this Transform parent)
        {
            GameObject go = parent.CreateFromInstance("Toggle");
            return new ScribbleUICheckbox(go);
        }

        public static GameObject GetChild(this GameObject go, string path)
        {
            return go.transform.Find(path).gameObject;
        }

        public static T GetComponent<T>(this GameObject go, string path) where T : Component
        {
            return go.GetChild(path).GetComponent<T>();
        }
    }
}