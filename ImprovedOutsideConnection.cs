using ICities;
using Harmony;
using UnityEngine;
using ColossalFramework.UI;

namespace ImprovedOutsideConnection
{
    public class ImprovedOutsideConnectionMod : LoadingExtensionBase, IUserMod
    {
        private readonly string version = "0.0.1";

        public string Name => "ImprovedOutsideConnection";

        public string Description => "Advanced options for outside connections.";

        public static bool InGame { get; internal set; } = false;

        private static string m_HarmonyIdentifier = "connection.outside.improved";

        HarmonyInstance m_HarmonyInstance = null;

        public static SettingsGUI m_SettingsGUI = null;

        private GameObject m_IOCGameObject = null;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            m_HarmonyInstance = HarmonyInstance.Create(m_HarmonyIdentifier);



            if (loading.loadingComplete)
            {
                switch (loading.currentMode)
                {
                    case AppMode.Game:
                        Init();
                        break;
                }
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                    Init();
                    break;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (InGame)
            {
                Deinit();
                m_HarmonyInstance = null;
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (!InGame)
                return;

            Deinit();
        }

        private void Init()
        {
            InGame = true;
            m_HarmonyInstance.PatchAll();
            OutsideConnectionSettingsManager.instance.SyncWithBuildingManager();

            //var cameraController = GameObject.FindObjectOfType<CameraController>();
            //m_SettingsGUI = cameraController.gameObject.AddComponent<SettingsGUI>();

            var objectOfType = UnityEngine.Object.FindObjectOfType<UIView>();
            m_IOCGameObject = new GameObject("IOCGameObject");
            m_IOCGameObject.transform.parent = objectOfType.transform;
            m_IOCGameObject.AddComponent<PanelExtenderOutsideConnections>();
        }

        private void Deinit()
        {
            try
            {
                m_HarmonyInstance.UnpatchAll(m_HarmonyIdentifier);

                GameObject.Destroy(m_SettingsGUI);
                m_SettingsGUI = null;

                if (m_IOCGameObject)
                    UnityEngine.Object.Destroy(m_IOCGameObject);
            }
            finally
            {
                InGame = false;
            }
        }
    }
}
