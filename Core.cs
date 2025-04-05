using MelonLoader;
using ModEnforcementAgency.MainMenu;
using ModEnforcementAgency.Managers.Items;
using ModEnforcementAgency.Managers;
using ModEnforcementAgency.Utils;
using ScheduleOne.UI.MainMenu;
using UnityEngine;
using HarmonyLib;

[assembly: MelonInfo(typeof(ModEnforcementAgency.Core), "ModEnforcementAgency", "0.1.1", "Moddy", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: HarmonyDontPatchAll]

namespace ModEnforcementAgency
{
    public class Core : MelonPlugin
    {
        public static Core Instance;
        internal static ModManager ModManager => ModManager.Instance; // Access to the ModManager instance for managing mods
        public const string VERSION = "0.1.1";

        private bool settingsTabAdded = false;
        private bool versionDisplayConfigured = false;

        public AssetBundle MEAContent { get; private set; }

        public MelonLogger.Instance logger;

        public override void OnApplicationStarted()
        {
            ModManager.RegisterMods(MelonBase.RegisteredMelons); // Register loaded mods with the ModManager

            PrefManager.Instance.Initialize();
            // Register mods after they're loaded
            PrefManager.Instance.RegisterAllMods();

            // Initialize the custom item system
            CustomItemManager.Initialize();

            // Load MEA content asset bundle
            MEAContent = AssetUtils.LoadAssetBundleFromResources("meacontent", MelonAssembly.Assembly);

        }

        public override void OnPreInitialization()
        {

        }

        public override void OnInitializeMelon()
        {
            Instance = this;
            logger = LoggerInstance;
            LoggerInstance.Msg("MEA v" + VERSION + " Initialized.");


            // Apply harmony patches
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.moddy.ModEnforcementAgency");
            ApplyPatches(harmony);

        }


        private void ApplyPatches(HarmonyLib.Harmony harmony)
        {
            var originalMethod = AccessTools.Method(typeof(SettingsScreen), "Start");
            var postfixMethod = AccessTools.Method(typeof(Core), "SettingsScreen_Start_Postfix");

            if (originalMethod != null && postfixMethod != null)
            {
                harmony.Patch(originalMethod, null, new HarmonyMethod(postfixMethod));
                LoggerInstance.Msg("Successfully patched SettingsScreen.Start");
            }
            else
            {
                LoggerInstance.Error($"Failed to patch SettingsScreen.Start - originalMethod={originalMethod != null}, postfixMethod={postfixMethod != null}");
            }

            var originalMainMenuMethod = AccessTools.Method(typeof(MainMenuRig), "LoadStuff");
            var postfixMainMenuMethod = AccessTools.Method(typeof(Core), "MainMenuScreen_Open_Postfix");

            if (originalMainMenuMethod != null && postfixMainMenuMethod != null)
            {
                harmony.Patch(originalMainMenuMethod, null, new HarmonyMethod(postfixMainMenuMethod));
                LoggerInstance.Msg("Successfully patched MainMenuRig.LoadStuff");
            }
            else
            {
                LoggerInstance.Error($"Failed to patch MainMenuRig.LoadStuff - originalMethod={originalMainMenuMethod != null}, postfixMethod={postfixMainMenuMethod != null}");
            }
        }

        // Postfix for SettingsScreen.Start

        private static void SettingsScreen_Start_Postfix(SettingsScreen __instance)
        {
            if (Instance != null && !Instance.settingsTabAdded)
            {
                Instance.LoggerInstance.Msg("Adding mod settings tab to settings screen");
                MelonCoroutines.Start(ModSettingsMenu.AddModCategory(__instance));
                Instance.settingsTabAdded = true;
            }
        }

        // Postfix for MainMenuRig.LoadStuff
        
        private static void MainMenuScreen_Open_Postfix(MainMenuRig __instance)
        {
            if (Core.Instance != null && !Core.Instance.versionDisplayConfigured)
            {
                MelonCoroutines.Start(FontUtils.LoadFonts());
                MelonCoroutines.Start(VersionDisplayComponent.AddVersionDisplayWithDelay());
                Core.Instance.versionDisplayConfigured = true;
                Core.Instance.LoggerInstance.Msg("Version display configured successfully");

            }
        }

    }
}