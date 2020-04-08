
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.Math;
using Harmony;

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
                                return false;   // Well, we skip the original code, because it isn't necessary and this way we safe some performance. Are there any good reasons not to do?
                            }
                            break;
                        }

                    case OutsideConnectionSettings.NameModeType.CustomRandom:
                        {
                            if (setting.RandomGenerationNames.Count != 0)
                            {
                                Randomizer r = new Randomizer(caller.Index);
                                __result = setting.RandomGenerationNames[r.Int32((uint)setting.RandomGenerationNames.Count)];
                                return false;   // Well, we skip the original code, because it isn't necessary and this way we safe some performance. Are there any good reasons not to do?
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

            return true;
        }
    }
}
