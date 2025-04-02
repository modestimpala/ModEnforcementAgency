using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using ModEnforcementAgency.Utils;

namespace ModEnforcementAgency.MainMenu
{

    [RegisterTypeInIl2Cpp]
    public class VersionDisplayComponent : MonoBehaviour
    {
        private static GameObject versionDisplay;

        private GameObject textObj;
        private Text versionText;

        public VersionDisplayComponent(IntPtr ptr) : base(ptr) { }

        public void SetVersion(string version)
        {
            try
            {
                // Create child text object
                textObj = new GameObject("VersionText");
                textObj.transform.SetParent(transform, false);

                // Add RectTransform
                RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0); // Bottom left
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.anchoredPosition = new Vector2(10, 10); // 10px from bottom left
                rectTransform.sizeDelta = new Vector2(200, 30);

                // Add RectTransform to text object as well
                RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
                textRectTransform.anchorMin = Vector2.zero;
                textRectTransform.anchorMax = Vector2.one;
                textRectTransform.offsetMin = Vector2.zero;
                textRectTransform.offsetMax = Vector2.zero;

                // Add background image 
                Image background = gameObject.AddComponent<Image>();
                background.color = new Color(0f, 0f, 0f, 0.5f);

                // Add text component
                versionText = textObj.AddComponent<Text>();
                versionText.text = $"MEA v{version}";
                versionText.fontSize = 14;
                versionText.color = Color.white;
                versionText.alignment = TextAnchor.MiddleCenter;

                versionText.font = FontUtils.normalFont;

                Core.Instance.LoggerInstance.Msg("Version display configured successfully");
            }
            catch (Exception ex)
            {
                Core.Instance.LoggerInstance.Error($"Error setting up version display: {ex.Message}");
            }
        }

        public static IEnumerator AddVersionDisplayWithDelay()
        {
            // Wait for a few frames to ensure everything is loaded
            for (int i = 0; i < 5; i++)
                yield return null;

            try
            {
                // Find the canvas
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();

                if (canvas != null)
                {
                    // Create version display if it doesn't exist
                    if (versionDisplay == null)
                    {
                        versionDisplay = new GameObject("MEAVersionDisplay");
                        versionDisplay.transform.SetParent(canvas.transform, false);

                        // Add our component
                        VersionDisplayComponent versionComponent = versionDisplay.AddComponent<VersionDisplayComponent>();
                        versionComponent.SetVersion(Core.VERSION);

                        Core.Instance.LoggerInstance.Msg("Added version display component");
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Instance.LoggerInstance.Error($"Error adding version display: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

