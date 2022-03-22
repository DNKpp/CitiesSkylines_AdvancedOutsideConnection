
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using HarmonyLib;

namespace AdvancedOutsideConnection.HarmonyPatches
{
	[HarmonyPatch(typeof(OutsideConnectionAI))]
	[HarmonyPatch("AddConnectionOffers")]
	class OutsideConnectionAddConnectionOffers
	{
		private static bool Prefix(ushort buildingID, ref int cargoCapacity, ref int residentCapacity, ref int touristFactor0, ref int touristFactor1, ref int touristFactor2, ref int dummyTrafficFactor)
		{
			if (OutsideConnectionSettingsManager.instance.SettingsDict.TryGetValue(buildingID, out OutsideConnectionSettings settings))
			{
				if (0 <= settings.DummyTrafficFactor)
					dummyTrafficFactor = settings.DummyTrafficFactor;

				if (0 <= settings.CargoCapacity)
					cargoCapacity = settings.CargoCapacity;

				if (0 <= settings.ResidentCapacity)
					residentCapacity = settings.ResidentCapacity;

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
