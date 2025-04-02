<p align="center" width="100%">
  <img src="icon.png">
</p>

# M.E.A.

WIP Readme, WIP Plugin.

ModEnforcementAgency is meant to lay out basic groundwork for modding. It allows for loading AssetBundles at runtime, adding custom items to game Registry, and injecting custom items into specific shops.

Custom Items completely untested on Multiplayer.

## Users:

Without another mod using MEA, the mod does not do much. It does, however, include a built in mod-viewer, that shows list of installed mods. (See Settings)

## Modders:

Make sure to include `Schedule I\MelonLoader\net6\UnityEngine.Il2CppAssetBundleManager.dll` and `ModEnforcementAgency.dll` as Dependencies for your project.

```
using ModEnforcementAgency.Managers.Items;
using ModEnforcementAgency.Utils;
public override void OnApplicationStarted()
{
    // Example of loading a custom item from an asset bundle
    // First, load asset bundle
    Il2CppAssetBundle bundle = AssetUtils.LoadAssetBundleFromResources("testbundle", MelonAssembly.Assembly);

    // Load PropertyItemDefinition from the bundle 
    StorableItemDefinition propertyItemDef = bundle.LoadAsset<PropertyItemDefinition>("TestItem");

    // Register it in the game's item manager
    CustomItemManager.RegisterItem(propertyItemDef);

    // Now add it to the shop, the api will wait to add it until the shop is loaded
    CustomItemManager.AddItemToShop(CustomItemManager.EShopType.GasStation, propertyItemDef.ID);
}
```

Please take careful note of special IL2Cpp types, like Il2CppAssetBundle. (and Lists)

AssetUtils contain functions for loading bundles from paths and embedded resources.

FontUtils caches some fonts for you to use for UI creation.

See GitHub Repo - Contributions welcomed and encouraged.

https://github.com/modestimpala/ModEnforcementAgency

[Join the Schedule 1 Modding Discord!](https://discord.gg/9Z5RKEYSzq)

