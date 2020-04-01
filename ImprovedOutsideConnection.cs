using ICities;

using ColossalFramework;
using Harmony;
using UnityEngine;

namespace ImprovedOutsideConnection
{
    public class ImprovedOutsideConnectionMod : LoadingExtensionBase, IUserMod
    {
        private readonly string version = "0.0.1";

        public string Name => "ImprovedOutsideConnection";

        public string Description => "Advanced options for outside connections.";

        HarmonyInstance m_HarmonyInstance;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            m_HarmonyInstance = HarmonyInstance.Create("connection.outside.improved");
            //m_HarmonyInstance.PatchAll();

            Debug.Log(Name + ": Hello, World!");
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            m_HarmonyInstance.PatchAll();

            OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();
        }
    }
}
