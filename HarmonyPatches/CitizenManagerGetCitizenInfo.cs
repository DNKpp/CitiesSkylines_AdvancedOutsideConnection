
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

#if DEBUG
using Harmony;

namespace AdvancedOutsideConnection.HarmonyPatches
{
	[HarmonyPatch(typeof(Citizen))]
	[HarmonyPatch("GetCitizenInfo")]
	class CitizenManagerGetCitizenInfo
	{
		private static bool Prefix(uint citizenID)
		{
			var citizen = CitizenManager.instance.m_citizens.m_buffer[citizenID];
			if ((citizen.m_flags & (Citizen.Flags.Tourist | Citizen.Flags.MovingIn)) == (Citizen.Flags.Tourist | Citizen.Flags.MovingIn))
			{
				Utils.Log("Citizen wealth: " + citizen.WealthLevel);
			}
			return true;
		}
	}
}
#endif
