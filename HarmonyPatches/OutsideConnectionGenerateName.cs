using ColossalFramework.Math;
using Harmony;
using UnityEngine;

namespace AdvancedOutsideConnection.HarmonyPatches
{
    [HarmonyPatch(typeof(OutsideConnectionAI))]
    [HarmonyPatch("GenerateName")]
    class OutsideConnectionGenerateName
    {
        private static bool Prefix(ushort buildingID, InstanceID caller, ref string __result)
        {
            var settings = OutsideConnectionSettingsManager.instance.SettingsDict;
            OutsideConnectionSettings setting = null;
            if (settings.TryGetValue(buildingID, out setting))
            {
                switch (setting.NameMode)
                {
                    case OutsideConnectionSettings.NameModeType.CustomSingle:
                        {
                            if (setting.NameText.Count != 0)
                            {
                                __result = setting.NameText[0];
                                return false;
                            }
                            break;
                        }

                    case OutsideConnectionSettings.NameModeType.CustomRandom:
                        {
                            if (setting.NameText.Count != 0)
                            {
                                Randomizer r = new Randomizer(caller.Index);
                                __result = setting.NameText[r.Int32((uint)setting.NameText.Count)];
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
                Debug.Log("AdvancedOutsideConnection: GenerateName: No settings for OutsideConnection found.");

            return true;    // Well, we skip the original code, because it isn't necessary and this way we safe some performance. Are there any good reasons not to do?
        }
    }
}
