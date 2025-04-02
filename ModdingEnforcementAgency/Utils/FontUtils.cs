using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModEnforcementAgency.Utils
{
    internal class FontUtils
    {
        public static Font italicFont;
        public static Font normalFont;
        public static Font boldFont;

        public static IEnumerator LoadFonts()
        {
            yield return null; // Ensure we wait for one frame to allow the scene to load properly
            // Load all fonts in the scene and return them as a list
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            List<Font> fontList = new List<Font>(fonts);
            if (fontList.Count == 0)
            {
                Core.Instance.LoggerInstance.Warning("No fonts found in the scene.");
                yield break; // Exit if no fonts are found
            }
            if (fontList.Count > 0)
            {
                italicFont = fontList[0];
                normalFont = fontList.Count > 1 ? fontList[1] : italicFont; // Fallback to italic if normal not found
                boldFont = fontList.Count > 2 ? fontList[2] : italicFont; // Fallback to italic if bold not found
                Core.Instance.LoggerInstance.Msg($"Fonts loaded: Italic: {italicFont.name}, Normal: {normalFont.name}, Bold: {boldFont.name}");
                yield break;
            }
        }
    }
}
