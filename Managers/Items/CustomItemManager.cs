using MelonLoader;
using UnityEngine;
using ModEnforcementAgency.Utils;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Shop;
using ScheduleOne;

namespace ModEnforcementAgency.Managers.Items
{
    public static class CustomItemManager
    {
        private static readonly Dictionary<string, ItemDefinition> CustomItems = new Dictionary<string, ItemDefinition>();
        private static readonly Dictionary<string, StorableItemDefinition> CustomStorableItems =
            new Dictionary<string, StorableItemDefinition>();
        private static readonly MelonLogger.Instance Logger = new MelonLogger.Instance("MEA.CustomItemAPI");
        private static bool registryInitialized = false;

        // Enum for all known shops in the game
        public enum EShopType
        {
            GasStation,
            HardwareStore,
            HardwareStoreNorth,
            GasStationCentral,
            WeedSupplier,
            MethSupplier,
            CokeSupplier,
            DarkMarket
        }

        // Shop path mapping structure
        private struct ShopPathMapping
        {
            public EShopType ShopType;
            public string Path;

            public ShopPathMapping(EShopType shopType, string path)
            {
                ShopType = shopType;
                Path = path;
            }
        }

        // Dictionary of shop enum to their ShopInterface components
        private static readonly Dictionary<EShopType, ShopInterface> CachedShops =
            new Dictionary<EShopType, ShopInterface>();

        // Known shop paths in the scene with their corresponding enums
        private static readonly ShopPathMapping[] KnownShopPaths = new ShopPathMapping[]
        {
            new ShopPathMapping(EShopType.GasStation, "UI/GasStationInterface"),
            new ShopPathMapping(EShopType.HardwareStore, "UI/HardwareStoreInterface"),
            new ShopPathMapping(EShopType.HardwareStoreNorth, "UI/HardwareStoreInterface (North Store)"),
            new ShopPathMapping(EShopType.GasStationCentral, "UI/GasStationInterface_Central"),
            new ShopPathMapping(EShopType.WeedSupplier, "UI/Supplier Stores/WeedSupplierInterface"),
            new ShopPathMapping(EShopType.MethSupplier, "UI/Supplier Stores/MethSupplierInterface"),
            new ShopPathMapping(EShopType.CokeSupplier, "UI/Supplier Stores/CokeSupplierInterface"),
            new ShopPathMapping(EShopType.DarkMarket, "UI/DarkMarketInterface")
        };

        // Storage for shop-specific item mappings
        private static Dictionary<EShopType, List<ShopItemMapping>> shopItemMappings =
            new Dictionary<EShopType, List<ShopItemMapping>>();

        // Helper class to store shop item mapping info
        private class ShopItemMapping
        {
            public string ItemId { get; set; }
            public bool OverridePrice { get; set; }
            public float Price { get; set; }
        }

        // Initialize the system when the mod loads
        public static void Initialize()
        {
            // Register for scene loaded event to hook the Registry when it's available
            MelonEvents.OnSceneWasLoaded.Subscribe((buildIndex, sceneName) =>
            {
                if (sceneName == "Main" && !registryInitialized)
                {
                    RegisterCachedItems();
                    // Cache shops and add items to them
                    CacheShopsAndAddItems();
                }
            });

            Logger.Msg("Custom Item API initialized");
        }

        // Register a custom item definition to be added to the game
        public static ItemDefinition RegisterItem(ItemDefinition itemDef)
        {
            // Store StorableItemDefinitions in our separate dictionary for type safety
            if (itemDef is StorableItemDefinition storableDef)
            {
                CustomStorableItems[itemDef.ID] = storableDef;
                Logger.Msg($"Stored {itemDef.ID} as StorableItemDefinition type: {storableDef.GetType().Name}");
            }

            if (CustomItems.ContainsKey(itemDef.ID))
            {
                Logger.Warning($"Item with ID {itemDef.ID} already exists! Returning existing item.");
                return CustomItems[itemDef.ID];
            }

            CustomItems.Add(itemDef.ID, itemDef);
            Logger.Msg($"Custom item registered: {itemDef.ID}");

            // If Registry is already initialized, add the item immediately
            if (registryInitialized && Registry.Instance != null)
            {
                try
                {
                    Registry.Instance.AddToRegistry(itemDef);
                    Logger.Msg($"Added {itemDef.ID} to game registry immediately");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to add item {itemDef.ID} to registry: {ex.Message}");
                }
            }

            return itemDef;
        }

        // Get a StorableItemDefinition by ID with proper type information
        public static StorableItemDefinition GetStorableItem(string itemId)
        {
            if (CustomStorableItems.TryGetValue(itemId, out StorableItemDefinition item))
            {
                return item;
            }

            // Try getting from registry and casting
            var baseItem = Registry.GetItem(itemId);
            if (baseItem is StorableItemDefinition storableItem)
            {
                return storableItem;
            }

            return null;
        }

        // Add an item to a specific shop
        public static void AddItemToShop(EShopType shopType, string itemId, float overridePrice = -1f)
        {
            // Store this information for when shop is loaded
            if (!shopItemMappings.ContainsKey(shopType))
            {
                shopItemMappings[shopType] = new List<ShopItemMapping>();
            }

            shopItemMappings[shopType].Add(new ShopItemMapping
            {
                ItemId = itemId,
                OverridePrice = overridePrice > 0,
                Price = overridePrice
            });

            Logger.Msg($"Queued item {itemId} to be added to shop {shopType}");

            // If shops are already cached, try to add it immediately
            if (registryInitialized && CachedShops.TryGetValue(shopType, out ShopInterface shop))
            {
                AddItemToShopInternal(shop, itemId, overridePrice > 0, overridePrice);

                // Refresh the shop if it's open
                if (shop.IsOpen)
                {
                    shop.RefreshShownItems();
                }
            }
        }

        // Load a sprite from an asset bundle for item icons
        public static Sprite LoadSpriteFromAssetBundle(string bundleName, string assetName,
            System.Reflection.Assembly resourceAssembly)
        {
            try
            {
                var bundle = AssetUtils.LoadAssetBundleFromResources(bundleName, resourceAssembly);
                if (bundle == null)
                    return null;

                // unload the bundle after loading the sprite
                return bundle.LoadAsset<Sprite>(assetName);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load sprite from asset bundle: {ex.Message}");
                return null;
            }
        }

        // Method to manually refresh a specific shop
        public static void RefreshShop(EShopType shopType)
        {
            if (CachedShops.TryGetValue(shopType, out ShopInterface shop))
            {
                shop.RefreshShownItems();
                Logger.Msg($"Refreshed shop {shopType}");
            }
            else
            {
                Logger.Warning($"Cannot refresh shop {shopType}: Shop not found in cache");
            }
        }

        // Cache all shop interfaces and add queued items to them
        private static void CacheShopsAndAddItems()
        {
            try
            {
                // Clear the cache first
                CachedShops.Clear();

                // Find and cache all known shops
                foreach (var shopMapping in KnownShopPaths)
                {
                    GameObject shopObj = GameObject.Find(shopMapping.Path);
                    if (shopObj != null)
                    {
                        ShopInterface shop = shopObj.GetComponent<ShopInterface>();
                        if (shop != null)
                        {
                            CachedShops[shopMapping.ShopType] = shop;
                            Logger.Msg($"Cached shop: {shopMapping.ShopType} from path {shopMapping.Path}");
                        }
                        else
                        {
                            Logger.Warning(
                                $"Shop GameObject found at {shopMapping.Path} but doesn't have ShopInterface component");
                        }
                    }
                    else
                    {
                        Logger.Warning($"Shop GameObject not found at path: {shopMapping.Path}");
                    }
                }

                // Add queued items to the shops
                foreach (var shopEntry in shopItemMappings)
                {
                    EShopType shopType = shopEntry.Key;
                    List<ShopItemMapping> items = shopEntry.Value;

                    if (CachedShops.TryGetValue(shopType, out ShopInterface shop))
                    {
                        foreach (var mapping in items)
                        {
                            AddItemToShopInternal(shop, mapping.ItemId, mapping.OverridePrice, mapping.Price);
                        }

                        // Refresh the shop
                        shop.RefreshShownItems();

                        Logger.Msg($"Added {items.Count} items to shop: {shopType}");
                    }
                    else
                    {
                        Logger.Warning($"Failed to add items to shop {shopType}: Shop not found in cache");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in CacheShopsAndAddItems: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Internal method to actually add an item to a shop
        private static void AddItemToShopInternal(ShopInterface shop, string itemId, bool overridePrice, float price)
        {
            try
            {
                // Try to get the item from Registry
                StorableItemDefinition item = GetStorableItem(itemId);

                if (item == null)
                {
                    Logger.Warning(
                        $"Cannot add item {itemId} to shop {shop.ShopName}: Item not found or not a StorableItemDefinition");
                    return;
                }

                // Check if item is already in the shop
                foreach (var listing2 in shop.Listings)
                {
                    if (listing2.Item.ID == itemId)
                    {
                        Logger.Msg($"Item {itemId} already in shop {shop.ShopName}");
                        return;
                    }
                }

                // Create a new listing
                ShopListing listing = new ShopListing();
                listing.Item = item;

                // Set price if overriding
                if (overridePrice)
                {
                    listing.OverridePrice = true;
                    listing.OverriddenPrice = price;
                }

                // Add to shop
                shop.Listings.Add(listing);

                // Create the UI for the listing
                shop.CreateListingUI(listing);

                Logger.Msg($"Added item {itemId} to shop {shop.ShopName}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding {itemId} to shop {shop.ShopName}: {ex.Message}");
            }
        }

        // Internal method to register all custom items with the Registry
        private static void RegisterCachedItems()
        {
            var registry = Registry.Instance;
            if (registry == null)
            {
                Logger.Error("Registry instance not found! Items will not be registered.");
                return;
            }

            foreach (var item in CustomItems.Values)
            {
                try
                {
                    registry.AddToRegistry(item);
                    Logger.Msg($"Added {item.ID} to game registry");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to register item {item.ID}: {ex.Message}");
                }
            }

            registryInitialized = true;
        }
    }
}