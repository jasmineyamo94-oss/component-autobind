using UnityEngine;
using UnityEngine.UI;
using JasmineYamo.ComponentAutoBind;

namespace JasmineYamo.ComponentAutoBind.Samples.BasicUGUI
{
    /// <summary>
    /// Creates a small UGUI hierarchy at runtime so the sample needs no scene asset.
    /// </summary>
    public sealed class BasicUGUIExample : MonoBehaviour
    {
        private void Start()
        {
            GameObject root = new GameObject("BasicAutoBindRoot");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            BasicAutoBindTarget target = root.AddComponent<BasicAutoBindTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();

            Button button = CreateButton(root.transform);
            Text statusText = CreateStatusText(root.transform);
            tool.m_BindComs.Add(button);
            tool.m_BindComs.Add(statusText);

            target.Initialize();
            target.SubmitButton.onClick.AddListener(() =>
            {
                target.StatusText.text = "Button clicked";
            });
        }

        private static Button CreateButton(Transform parent)
        {
            GameObject buttonObject = new GameObject("Btn_Submit");
            buttonObject.transform.SetParent(parent, false);
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, -40f);
            rectTransform.sizeDelta = new Vector2(220f, 48f);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.45f, 0.68f);
            Button button = buttonObject.AddComponent<Button>();
            Text label = CreateText(buttonObject.transform, "Submit");
            label.color = Color.white;
            return button;
        }

        private static Text CreateStatusText(Transform parent)
        {
            GameObject textObject = new GameObject("Txt_Status");
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, 40f);
            rectTransform.sizeDelta = new Vector2(360f, 48f);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.text = "Ready";
            return text;
        }

        private static Text CreateText(Transform parent, string value)
        {
            GameObject textObject = new GameObject("Label");
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.text = value;
            return text;
        }
    }
}
