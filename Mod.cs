
//		  Copyright Dominic Koepke 2020 - 2021.
// Distributed under the Boost Software License, Version 1.0.
//	(See accompanying file LICENSE_1_0.txt or copy at
//		  https://www.boost.org/LICENSE_1_0.txt)

using ICities;
using CitiesHarmony.API;

namespace AdvancedOutsideConnection
{
	public class Mod : IUserMod
	{
		private static readonly string version = "1.4";

		public static string ModName => "AdvancedOutsideConnection";

		public string Name { get { return ModName; } }

		public string Description => "Advanced options for outside connections.";


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }
    }
}
