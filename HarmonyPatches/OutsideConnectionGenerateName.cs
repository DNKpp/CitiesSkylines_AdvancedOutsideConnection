using ColossalFramework.Math;
using Harmony;
using UnityEngine;

namespace ImprovedOutsideConnection.HarmonyPatches
{
    [HarmonyPatch(typeof(OutsideConnectionAI))]
    [HarmonyPatch("GenerateName")]
    class OutsideConnectionGenerateName
    {
        private static bool Prefix(ushort buildingID, ref string __result)
        {
            var settings = OutsideConnectionSettingsManager.instance.SettingsDict;
            OutsideConnectionSettings setting = null;
            if (settings.TryGetValue(buildingID, out setting))
            {
                switch (setting.NameMode)
                {
                    case OutsideConnectionSettings.NameModeType.CustomSingle:
                        {
                            var textArr = setting.NameText.Split(';');
                            if (textArr.Length != 0)
                            {
                                __result = textArr[0];
                                return false;
                            }
                            break;
                        }

                    case OutsideConnectionSettings.NameModeType.CustomRandom:
                        {
                            var textArr = setting.NameText.Split(';');
                            if (textArr.Length != 0)
                            {
                                Randomizer r = new Randomizer(buildingID);
                                __result = textArr[r.Int32(0, textArr.Length - 1)];
                                return false;
                            }
                            break;
                        }

                    case OutsideConnectionSettings.NameModeType.Vanilla:    // just follow as usual
                    default:
                        break;
                }
            }
            else
                Debug.Log("ImprovedOutsideConnection: GenerateName: No settings for OutsideConnection found.");

            return true;    // Well, we skip the original code, because it isn't necessary and this way we safe some performance. Are there any good reasons not to do?
        }
    }
}
