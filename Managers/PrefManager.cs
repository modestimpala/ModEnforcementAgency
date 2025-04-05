using MelonLoader;

namespace ModEnforcementAgency.Managers
{
    internal class PrefManager
    {
        // Singleton instance
        private static PrefManager _instance;
        public static PrefManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PrefManager();
                }
                return _instance;
            }
        }

        // Preferences categories
        private MelonPreferences_Category modConfigCategory;
        private MelonPreferences_Category userSettingsCategory;

        // Preference entries
        private MelonPreferences_Entry<bool> enabledModsAutoSave;
        private MelonPreferences_Entry<string> lastLoadedProfile;
        private Dictionary<string, MelonPreferences_Entry<bool>> modEnabledStateEntries;

        // Custom save paths
        private const string MOD_PROFILES_DIR = "UserData/ModProfiles";
        private const string CURRENT_PROFILE_FILENAME = "CurrentProfile.cfg";

        // Initialize the preference manager
        public void Initialize()
        {
            // Create directories if they don't exist
            if (!Directory.Exists(MOD_PROFILES_DIR))
            {
                Directory.CreateDirectory(MOD_PROFILES_DIR);
            }

            // Initialize categories
            modConfigCategory = MelonPreferences.CreateCategory("MEA_ModConfiguration");
            userSettingsCategory = MelonPreferences.CreateCategory("MEA_UserSettings");
            
            // Set custom file path for mod configuration
            modConfigCategory.SetFilePath(Path.Combine(MOD_PROFILES_DIR, CURRENT_PROFILE_FILENAME));

            // Initialize preference entries
            enabledModsAutoSave = userSettingsCategory.CreateEntry("EnabledModsAutoSave", true);
            lastLoadedProfile = userSettingsCategory.CreateEntry("LastLoadedProfile", "Default");
            
            // Initialize mod enabled states
            modEnabledStateEntries = new Dictionary<string, MelonPreferences_Entry<bool>>();
            
            // Register events
            MelonEvents.OnApplicationQuit.Subscribe(SaveAllPreferences);
        }

        // Save all preferences
        public void SaveAllPreferences()
        {
            MelonPreferences.Save();
            modConfigCategory.SaveToFile();
        }

        // Register a mod with preferences
        public void RegisterMod(MelonBase mod)
        {
            if (mod == null) return;
            
            string modID = GetModIdentifier(mod);
            
            // Create entry for this mod if it doesn't exist
            if (!modEnabledStateEntries.ContainsKey(modID))
            {
                var entry = modConfigCategory.CreateEntry(modID, true);
                modEnabledStateEntries.Add(modID, entry);
            }
        }

        // Register all mods from ModManager
        public void RegisterAllMods()
        {
            var mods = ModManager.Instance.LoadedMods;
            if (mods != null)
            {
                foreach (var mod in mods)
                {
                    RegisterMod(mod);
                }
            }
        }

        // Get mod enabled state
        public bool IsModEnabled(MelonBase mod)
        {
            if (mod == null) return false;
            
            string modID = GetModIdentifier(mod);
            
            // Check if mod exists in preferences
            if (modEnabledStateEntries.TryGetValue(modID, out var entry))
            {
                return entry.Value;
            }
            
            // Register mod if not found
            RegisterMod(mod);
            return true; // Default to enabled
        }

        // Set mod enabled state
        public void SetModEnabled(MelonBase mod, bool enabled)
        {
            if (mod == null) return;
            
            string modID = GetModIdentifier(mod);
            
            // Check if mod exists in preferences
            if (modEnabledStateEntries.TryGetValue(modID, out var entry))
            {
                entry.Value = enabled;
            }
            else
            {
                // Register mod if not found
                RegisterMod(mod);
                modEnabledStateEntries[modID].Value = enabled;
            }
            
            // Auto-save if enabled
            if (enabledModsAutoSave.Value)
            {
                modConfigCategory.SaveToFile();
            }
        }

        // Create a new mod profile
        public void CreateProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return;
            
            // Save current profile first
            modConfigCategory.SaveToFile();
            
            // Set new profile path
            string profilePath = Path.Combine(MOD_PROFILES_DIR, $"{profileName}.cfg");
            modConfigCategory.SetFilePath(profilePath, autoload: false);
            
            // Save current mod states to new profile
            modConfigCategory.SaveToFile();
            
            // Update last loaded profile
            lastLoadedProfile.Value = profileName;
            userSettingsCategory.SaveToFile();
        }

        // Load a mod profile
        public void LoadProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return;
            
            string profilePath = Path.Combine(MOD_PROFILES_DIR, $"{profileName}.cfg");
            
            // Check if profile exists
            if (!File.Exists(profilePath))
            {
                MelonLogger.Warning($"Profile '{profileName}' does not exist. Creating new profile.");
                CreateProfile(profileName);
                return;
            }
            
            // Set and load profile
            modConfigCategory.SetFilePath(profilePath);
            
            // Refresh mod enabled state entries (references can change after loading)
            RefreshModEnabledStateEntries();
            
            // Update last loaded profile
            lastLoadedProfile.Value = profileName;
            userSettingsCategory.SaveToFile();
        }

        // Get list of available profiles
        public List<string> GetAvailableProfiles()
        {
            List<string> profiles = new List<string>();
            
            if (Directory.Exists(MOD_PROFILES_DIR))
            {
                foreach (string file in Directory.GetFiles(MOD_PROFILES_DIR, "*.cfg"))
                {
                    string profileName = Path.GetFileNameWithoutExtension(file);
                    profiles.Add(profileName);
                }
            }
            
            return profiles;
        }

        // Helper method to get consistent mod identifier
        private string GetModIdentifier(MelonBase mod)
        {
            // Use AssemblyName_Info.Name as a unique identifier
            return $"{mod.Info.Name}_{mod.MelonAssembly.Assembly.GetName().Name}";
        }

        // Helper method to refresh mod enabled state entries after loading a profile
        private void RefreshModEnabledStateEntries()
        {
            modEnabledStateEntries.Clear();
            
            foreach (var entry in modConfigCategory.Entries)
            {
                if (entry is MelonPreferences_Entry<bool> boolEntry)
                {
                    modEnabledStateEntries.Add(entry.Identifier, boolEntry);
                }
            }
            
            // Register any missing mods
            RegisterAllMods();
        }

        // Private constructor to prevent instantiation
        private PrefManager()
        {
            modEnabledStateEntries = new Dictionary<string, MelonPreferences_Entry<bool>>();
        }
    }
}