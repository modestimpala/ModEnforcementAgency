using UnityEngine;
using System.Reflection;
using System.IO;
using System;

namespace ModEnforcementAgency.Utils
{
    public static class AssetUtils
    {
        public static AssetBundle LoadAssetBundleFromResources(string bundleName, Assembly resourceAssembly)
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException("Parameter resourceAssembly can not be null.");
            }
            string resourceName = null;
            try
            {
                resourceName = resourceAssembly.GetManifestResourceNames().Single(str => str.EndsWith(bundleName));
            }
            catch (Exception) { }
            if (resourceName == null)
            {
                Core.Instance.LoggerInstance.Error($"AssetBundle {bundleName} not found in assembly manifest");
                return null;
            }
            AssetBundle ret;
            using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Core.Instance.LoggerInstance.Error($"Failed to load AssetBundle {bundleName} from stream.");
                    return null;
                }
                ret = AssetBundle.LoadFromStream(stream);
            }
            return ret;
        }

        public static AssetBundle LoadAssetBundleFromAbsolutePath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                Core.Instance.LoggerInstance.Error("Absolute path cannot be null or empty.");
                return null;
            }

            if (!File.Exists(absolutePath))
            {
                Core.Instance.LoggerInstance.Error($"AssetBundle file not found at path: {absolutePath}");
                return null;
            }

            try
            {
                // Load the AssetBundle from the absolute path
                using FileStream fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
                using MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                AssetBundle assetBundle = AssetBundle.LoadFromStream(memoryStream);

                if (assetBundle == null)
                {
                    Core.Instance.LoggerInstance.Error($"Failed to load AssetBundle from path: {absolutePath}");
                    return null;
                }

                return assetBundle;
            }
            catch (Exception ex)
            {
                Core.Instance.LoggerInstance.Error($"Exception loading AssetBundle from path {absolutePath}: {ex.Message}");
                return null;
            }
        }

        public static AssetBundle LoadAssetBundleFromRelativePath(string relativePath, Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("Parameter assembly cannot be null.");
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                Core.Instance.LoggerInstance.Error("Relative path cannot be null or empty.");
                return null;
            }

            try
            {
                // Get the directory of the assembly
                string assemblyDirectory = Path.GetDirectoryName(assembly.Location);

                // Combine the assembly directory with the relative path
                string fullPath = Path.Combine(assemblyDirectory, relativePath);

                // Use the absolute path method to load the asset bundle
                return LoadAssetBundleFromAbsolutePath(fullPath);
            }
            catch (Exception ex)
            {
                Core.Instance.LoggerInstance.Error($"Exception resolving relative path: {ex.Message}");
                return null;
            }
        }
    }
}