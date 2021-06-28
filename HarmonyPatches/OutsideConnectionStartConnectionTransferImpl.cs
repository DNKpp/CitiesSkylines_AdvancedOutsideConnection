
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using Harmony;

namespace AdvancedOutsideConnection.HarmonyPatches
{
	[HarmonyPatch(typeof(OutsideConnectionAI))]
	[HarmonyPatch("StartConnectionTransferImpl")]
	class OutsideConnectionStartConnectionTransferImpl
	{
		private static bool Prefix(ushort buildingID, ref int touristFactor0, ref int touristFactor1, ref int touristFactor2)
		{
			if (OutsideConnectionSettingsManager.instance.SettingsDict.TryGetValue(buildingID, out OutsideConnectionSettings settings))
			{
				if (settings.TouristFactors != null && 3 <= settings.TouristFactors.Length)
				{
					touristFactor0 = settings.TouristFactors[0];
					touristFactor1 = settings.TouristFactors[1];
					touristFactor2 = settings.TouristFactors[2];
				}
			}
			return true;
		}
	}
}
