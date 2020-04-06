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
                            if (setting.SingleGenerationName != string.Empty)
                            {
                                __result = setting.SingleGenerationName;
                                return false;
                            }
                            break;
                        }

                    case OutsideConnectionSettings.NameModeType.CustomRandom:
                        {
                            if (setting.RandomGenerationNames.Count != 0)
                            {
                                Randomizer r = new Randomizer(caller.Index);
                                __result = setting.RandomGenerationNames[r.Int32((uint)setting.RandomGenerationNames.Count)];
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
                Utils.Log("AdvancedOutsideConnection: GenerateName: No settings for OutsideConnection found.");

            return true;    // Well, we skip the original code, because it isn't necessary and this way we safe some performance. Are there any good reasons not to do?
        }
    }
}
