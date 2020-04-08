
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.Math;
using Harmony;

namespace AdvancedOutsideConnection.HarmonyPatches
{
    [HarmonyPatch(typeof(OutsideConnectionAI))]
    [HarmonyPatch("AddConnectionOffers")]
    class OutsideConnectionAddConnectionOffers
    {
        private static bool Prefix(ushort buildingID, ref int dummyTrafficFactor)
        {
            if (OutsideConnectionSettingsManager.instance.SettingsDict.TryGetValue(buildingID, out OutsideConnectionSettings settings))
            {
                if (0 <= settings.DummyTrafficFactor)
                    dummyTrafficFactor = settings.DummyTrafficFactor;
            }
            return true;
        }
    }
}
