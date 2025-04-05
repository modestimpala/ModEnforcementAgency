using MelonLoader;
using System.Collections.ObjectModel;

namespace ModEnforcementAgency.Managers
{
    internal class ModManager
    {
        // Singleton instance
        private static ModManager _instance;
        public static ModManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModManager();
                }
                return _instance;
            }
        }

        // List to hold loaded mods
        public List<MelonBase> LoadedMods { get; private set; }

        internal void RegisterMods(ReadOnlyCollection<MelonBase> loadedAssemblies)
        {
            // Initialize the LoadedMods list if it's null
            if (LoadedMods == null)
            {
                LoadedMods = new List<MelonBase>();
            }
            // Clear the existing list to avoid duplicates
            LoadedMods.Clear();
            // Register each loaded mod
            foreach (var melon in loadedAssemblies)
            {
                if (melon != null)
                {
                    LoadedMods.Add(melon);
                }
                else
                {
                    Core.Instance.LoggerInstance.Warning("Encountered a null mod during registration");
                }
            }

        }

        // Private constructor to prevent instantiation
        private ModManager()
        {
            // Initialize the LoadedMods list
            LoadedMods = new List<MelonBase>();
        }

    }
}
