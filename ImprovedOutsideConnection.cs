using ICities;
using Harmony;
using UnityEngine;

namespace ImprovedOutsideConnection
{
    public class ImprovedOutsideConnectionMod : LoadingExtensionBase, IUserMod
    {
        private readonly string version = "0.0.1";

        public string Name => "ImprovedOutsideConnection";

        public string Description => "Advanced options for outside connections.";

        public static bool InGame { get; internal set; } = false;

        HarmonyInstance m_HarmonyInstance = null;

        public static SettingsGUI m_SettingsGUI = null;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            m_HarmonyInstance = HarmonyInstance.Create("connection.outside.improved");
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                    InGame = true;

                    m_HarmonyInstance.PatchAll();

                    OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();

                    var cameraController = GameObject.FindObjectOfType<CameraController>();
                    m_SettingsGUI = cameraController.gameObject.AddComponent<SettingsGUI>();
                    break;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (InGame)
            {
                m_HarmonyInstance.UnpatchAll();
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (!InGame)
                return;

            try
            {
                m_HarmonyInstance.UnpatchAll();
                m_HarmonyInstance = null;

                GameObject.Destroy(m_SettingsGUI);
                m_SettingsGUI = null;
            }
            finally
            {
                InGame = false;
            }
        }
    }
}
