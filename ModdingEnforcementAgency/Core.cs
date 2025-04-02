using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.UI.MainMenu;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Product;
using ModEnforcementAgency.Utils;
using ModEnforcementAgency.MainMenu;
using ModEnforcementAgency.Managers;
using ModEnforcementAgency.Managers.Items;


[assembly: MelonInfo(typeof(ModEnforcementAgency.Core), "ModEnforcementAgency", "0.1.0", "Moddy", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: HarmonyDontPatchAll]

namespace ModEnforcementAgency
{
    public class Core : MelonPlugin
    {
        public static Core Instance;
        internal static ModManager ModManager => ModManager.Instance; // Access to the ModManager instance for managing mods
        public const string VERSION = "0.1.0";
        
        private bool settingsTabAdded = false;
        private bool versionDisplayConfigured = false;

        public Il2CppAssetBundle MEAContent { get; private set; }

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

            // Register our custom component
            ClassInjector.RegisterTypeInIl2Cpp<VersionDisplayComponent>();

            // Apply harmony patches
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.moddy.ModEnforcementAgency");
            ApplyPatches(harmony);
            
        }


        private void ApplyPatches(HarmonyLib.Harmony harmony)
        {
            // Hook into the SettingsScreen.Start method
            var originalMethod = typeof(SettingsScreen).GetMethod("Start", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var postfixMethod = typeof(Core).GetMethod("SettingsScreen_Start_Postfix", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            if (originalMethod != null && postfixMethod != null)
            {
                harmony.Patch(originalMethod, null, new HarmonyMethod(postfixMethod));
                LoggerInstance.Msg("Successfully patched SettingsScreen.Start");
            }
            else
            {
                LoggerInstance.Error("Failed to patch SettingsScreen.Start");
            }

            var originalMainMenuMethod = typeof(MainMenuRig).GetMethod("LoadStuff", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var postfixMainMenuMethod = typeof(Core).GetMethod("MainMenuScreen_Open_Postfix", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            if (originalMainMenuMethod != null && postfixMainMenuMethod != null)
            {
                harmony.Patch(originalMainMenuMethod, null, new HarmonyMethod(postfixMainMenuMethod));
                LoggerInstance.Msg("Successfully patched MainMenuScreen.Open");
            }
            else
            {
                LoggerInstance.Error("Failed to patch MainMenuScreen.Open");
            }
        }

        // Postfix for SettingsScreen.Start
        [HarmonyPatch(typeof(SettingsScreen), "Start")]
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
        [HarmonyPatch(typeof(MainMenuRig), "LoadStuff")]
        private static void MainMenuScreen_Open_Postfix(MainMenuScreen __instance)
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