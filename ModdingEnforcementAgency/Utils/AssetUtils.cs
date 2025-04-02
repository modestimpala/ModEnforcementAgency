using UnityEngine;
using System.Reflection;
using System.IO;
using System;

namespace ModEnforcementAgency.Utils
{
    public static class AssetUtils
    {
        public static Il2CppAssetBundle LoadAssetBundleFromResources(string bundleName, Assembly resourceAssembly)
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
            Il2CppAssetBundle ret;
            using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Core.Instance.LoggerInstance.Error($"Failed to load AssetBundle {bundleName} from stream.");
                    return null;
                }
                // Convert Stream stream to Il2cppSystem.IO.Stream
                Il2CppSystem.IO.Stream stream1 = new Il2CppSystem.IO.MemoryStream();
                // Have to write manually I guess, I'm not sure if there is a way to convert a stream to Il2CppSystem.IO.Stream or get a manifest stream in that type
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream1.Write(buffer, 0, bytesRead);
                }
                stream1.Position = 0; // Reset the position of the stream to the beginning
                // Load the asset bundle from the stream
                ret = Il2CppAssetBundleManager.LoadFromStream(stream1);
            }
            return ret;
        }

        public static Il2CppAssetBundle LoadAssetBundleFromAbsolutePath(string absolutePath)
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
                // Read all bytes from the file
                byte[] assetBundleData = File.ReadAllBytes(absolutePath);

                // Create an Il2Cpp memory stream
                Il2CppSystem.IO.MemoryStream il2cppStream = new Il2CppSystem.IO.MemoryStream(assetBundleData);

                // Load the asset bundle from the stream
                Il2CppAssetBundle assetBundle = Il2CppAssetBundleManager.LoadFromStream(il2cppStream);

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

        public static Il2CppAssetBundle LoadAssetBundleFromRelativePath(string relativePath, Assembly assembly)
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