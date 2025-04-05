using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using ModEnforcementAgency.Utils;
using ScheduleOne.UI.MainMenu;
using TMPro;

namespace ModEnforcementAgency.MainMenu
{
    internal class ModSettingsMenu
    {
        public static GameObject templateMod;

        public static IEnumerator AddModCategory(SettingsScreen settingsScreen)
        {
            // Wait just one frame to ensure everything is initialized
            yield return null;

            try
            {
                if (settingsScreen == null)
                {
                    Core.Instance.LoggerInstance.Error("SettingsScreen is null");
                    yield break;
                }

                // Get the existing categories array
                var categories = settingsScreen.Categories;
                if (categories == null || categories.Length == 0)
                {
                    Core.Instance.LoggerInstance.Error("Categories array is null or empty");
                    yield break;
                }

                // Create a new category
                var modCategory = new SettingsScreen.SettingsCategory();

                // Get the second category's button as a reference. First one is already "active" by default so we skip it.
                Button firstButton = categories[1].Button;
                if (firstButton == null)
                {
                    Core.Instance.LoggerInstance.Error("First category button is null");
                    yield break;
                }

                // Get the parent GameObject and use that instead of trying to access transform.parent directly
                GameObject buttonParentObj = firstButton.gameObject.transform.parent.gameObject;

                // Create a button for the mod tab (duplicate an existing one)
                GameObject buttonObj = GameObject.Instantiate(firstButton.gameObject, buttonParentObj.transform);
                buttonObj.name = "ModSettingsButton";
                Button button = buttonObj.GetComponent<Button>();

                // Set the button text to "Mods"
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Mods";
                }

                // Get the first category's panel as a reference
                GameObject firstPanel = categories[0].Panel;
                if (firstPanel == null)
                {
                    Core.Instance.LoggerInstance.Error("First category panel is null");
                    yield break;
                }

                // Get the parent GameObject of the panel
                GameObject panelParentObj = firstPanel.transform.parent.gameObject;

                
                // Instantiate the prefab from the asset bundle
                GameObject uiPrefab = GameObject.Instantiate(Core.Instance.MEAContent.LoadAsset<GameObject>("UI_InstalledMods"));
                uiPrefab.transform.SetParent(panelParentObj.transform, false);
                uiPrefab.name = "ModSettingsPanel";

                // Find the Content transform where we'll add our mod entries
                Transform contentTransform = uiPrefab.transform.Find("Scroll View/Viewport/Content");

                if (contentTransform == null)
                {
                    Core.Instance.LoggerInstance.Error("Content transform not found in UI_InstalledMods prefab");
                    yield break;
                }

                //Get TemplateMod gameObject
                templateMod = contentTransform.transform.Find("TemplateMod").gameObject;
                templateMod.SetActive(false);
                // This is a UI panel that will be used as a template for each mod entry

                // Add mods to content
                int modCount = 0;

                foreach (var mod in MelonBase.RegisteredMelons)
                {
                    if (mod == null)
                    {
                        Core.Instance.LoggerInstance.Warning("Encountered a null mod during mod settings UI creation");
                        continue;
                    }

                    // Create a ModInfoDisplay for each loaded mod
                    ModInfoDisplay modDisplay = new ModInfoDisplay(mod, contentTransform.gameObject);
                    modCount++;
                }

               

                // Assign button and panel to the new category
                modCategory.Button = button;
                modCategory.Panel = uiPrefab;

                // Initially hide the panel
                uiPrefab.SetActive(false);

                // Add the new category to the settings screen
                var newCategories = new SettingsScreen.SettingsCategory[categories.Length + 1];
                Array.Copy(categories, newCategories, categories.Length);
                newCategories[categories.Length] = modCategory;
                

                // Assign the new array
                settingsScreen.Categories = newCategories;

                // Set up the button click event
                int categoryIndex = categories.Length;
                button.onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    settingsScreen.ShowCategory(categoryIndex);
                }));

                Core.Instance.LoggerInstance.Msg("Added mod settings tab");
            }
            catch (Exception ex)
            {
                Core.Instance.LoggerInstance.Error($"Error adding mod settings tab: {ex.Message}\n{ex.StackTrace}");
            }
        }

    }

    internal class ModInfoDisplay
    {
        private MelonBase mod;
        private GameObject modInfoContainer;

        // Height of each mod entry - reduced for better compactness
        public float PreferredHeight { get; private set; } = 70f;

        public ModInfoDisplay(MelonBase mod, GameObject parent)
        {
            this.mod = mod;

            // Duplicate the template mod entry
            modInfoContainer = GameObject.Instantiate(ModSettingsMenu.templateMod, parent.transform);
            modInfoContainer.SetActive(true);

            // Set the name of the mod entry
            modInfoContainer.name = $"ModInfo_{mod.Info.Name}";

            // Create layout for the content
            GameObject contentLayout = new GameObject("ContentLayout");
            contentLayout.transform.SetParent(modInfoContainer.transform, false);

            RectTransform contentRect = contentLayout.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -10);

            // Create mod name text
            GameObject nameObj = new GameObject("ModName");
            nameObj.transform.SetParent(contentLayout.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0, 1);
            nameRect.sizeDelta = new Vector2(0, 25);
            nameRect.anchoredPosition = Vector2.zero;

            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = mod.Info.Name;
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            nameText.font = FontUtils.boldFont;

            // Create version text
            GameObject versionObj = new GameObject("ModVersion");
            versionObj.transform.SetParent(contentLayout.transform, false);

            RectTransform versionRect = versionObj.AddComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0, 1);
            versionRect.anchorMax = new Vector2(1, 1);
            versionRect.pivot = new Vector2(0, 1);
            versionRect.sizeDelta = new Vector2(0, 18);
            versionRect.anchoredPosition = new Vector2(0, -25);

            Text versionText = versionObj.AddComponent<Text>();
            versionText.text = $"Version: {mod.Info.Version}";
            versionText.fontSize = 14;
            versionText.alignment = TextAnchor.MiddleLeft;
            versionText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            versionText.font = FontUtils.normalFont;
            versionText.resizeTextForBestFit = true;

            // Create author text
            GameObject authorObj = new GameObject("ModAuthor");
            authorObj.transform.SetParent(contentLayout.transform, false);

            RectTransform authorRect = authorObj.AddComponent<RectTransform>();
            authorRect.anchorMin = new Vector2(0, 1);
            authorRect.anchorMax = new Vector2(1, 1);
            authorRect.pivot = new Vector2(0, 1);
            authorRect.sizeDelta = new Vector2(0, 18);
            authorRect.anchoredPosition = new Vector2(0, -43);

            Text authorText = authorObj.AddComponent<Text>();
            authorText.text = $"Author: {mod.Info.Author}";
            authorText.fontSize = 14;
            authorText.alignment = TextAnchor.MiddleLeft;
            authorText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            authorText.font = FontUtils.normalFont;
            authorText.resizeTextForBestFit = true;

            // Create assembly path text
            GameObject assemblyObj = new GameObject("ModAssembly");
            assemblyObj.transform.SetParent(contentLayout.transform, false);

            RectTransform assemblyRect = assemblyObj.AddComponent<RectTransform>();
            assemblyRect.anchorMin = new Vector2(0, 1);
            assemblyRect.anchorMax = new Vector2(1, 1);
            assemblyRect.pivot = new Vector2(0, 1);
            assemblyRect.sizeDelta = new Vector2(0, 18);
            assemblyRect.anchoredPosition = new Vector2(0, -61);

            Text assemblyText = assemblyObj.AddComponent<Text>();
            string assemblyPath = System.IO.Path.GetFileName(mod.MelonAssembly.Location);
            assemblyText.text = $"Assembly: {assemblyPath}";
            assemblyText.fontSize = 12;
            assemblyText.alignment = TextAnchor.MiddleLeft;
            assemblyText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            assemblyText.font = FontUtils.normalFont;
        }
    }
}