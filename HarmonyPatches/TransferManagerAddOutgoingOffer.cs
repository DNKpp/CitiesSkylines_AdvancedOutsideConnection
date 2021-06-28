
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using Harmony;
using System;
using static TransferManager;

namespace AdvancedOutsideConnection.HarmonyPatches
{
	[HarmonyPatch(typeof(TransferManager))]
	[HarmonyPatch("AddOutgoingOffer")]
	class TransferManagerAddOutgoingOffer
	{
		private static bool Prefix(ref TransferReason material, TransferOffer offer)
		{
			if (offer.m_object.Building != 0 && OutsideConnectionSettingsManager.instance.SettingsDict.TryGetValue(offer.m_object.Building, out OutsideConnectionSettings settings))
			{
				//var oldMat = material;
				Utils.ModMaterialIfNecessary(ref material, settings.ImportResourceRatio, Utils.ImportResources);
				//if (oldMat != material)
				//	Utils.Log("Import resource from connection named: " + settings.Name + " Switched from material: " + oldMat + " to material: " + material);
			}
			return true;
		}
	}
}
