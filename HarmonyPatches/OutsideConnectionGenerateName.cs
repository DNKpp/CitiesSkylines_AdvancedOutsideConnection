
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.Math;
using HarmonyLib;

namespace AdvancedOutsideConnection.HarmonyPatches
{
	[HarmonyPatch(typeof(OutsideConnectionAI))]
	[HarmonyPatch("GenerateName")]
	class OutsideConnectionGenerateName
	{
		private static void Postfix(ushort buildingID, InstanceID caller, ref string __result)
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
								__result = setting.SingleGenerationName;
							break;
						}

					case OutsideConnectionSettings.NameModeType.CustomRandom:
						{
							if (setting.RandomGenerationNames.Count != 0)
							{
								Randomizer r = new Randomizer(caller.Index);
								__result = setting.RandomGenerationNames[r.Int32((uint)setting.RandomGenerationNames.Count)];
							}
							break;
						}

					case OutsideConnectionSettings.NameModeType.Vanilla:	// just follow as usual
					default:
						break;
				}
			}
			else
				Utils.Log("AdvancedOutsideConnection: GenerateName: No settings for OutsideConnection found.");
		}
	}
}
