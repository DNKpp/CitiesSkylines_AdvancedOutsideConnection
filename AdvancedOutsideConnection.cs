using ICities;
using Harmony;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Collections.Generic;

namespace AdvancedOutsideConnection
{
    public class AdvancedOutsideConnectionMod : LoadingExtensionBase, IUserMod
    {
        private readonly string version = "0.0.1";

        public string Name => "AdvancedOutsideConnection";

        public string Description => "Advanced options for outside connections.";

        public static bool InGame { get; internal set; } = false;

        private static string m_HarmonyIdentifier = "connection.outside.advanced";

        HarmonyInstance m_HarmonyInstance = null;

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
                        Init(true);
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
                    Init(false);
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

        private void Init(bool late)
        {
            InGame = true;
            m_HarmonyInstance.PatchAll();

            // This is needed in cases where the mod is hot reloaded. Due to its implementation, the SerializableDataExtension OnCreated function will only be called during game loads.
            // This way we manually force the call, so we can relay on correct behavior afterwards.
            // Remark: Potentially should only be active in debug mode?
            if (late)
            {
                var serDataWrapper = SimulationManager.instance.m_SerializableDataWrapper;
                if (serDataWrapper != null)
                {
                    var serializableDataExtensions = Traverse.Create<SerializableDataExtension>().Field("m_SerializableDataExtensions").GetValue<List<ISerializableDataExtension>>();
                    serializableDataExtensions = PluginManager.instance.GetImplementations<ISerializableDataExtension>();
                    foreach (var extension in serializableDataExtensions)
                    {
                        var own = extension as SerializableDataExtension;
                        if (own != null)
                            own.OnCreated(serDataWrapper);
                    }
                }
            }
            SerializableDataExtension.instance.Loaded = true;

            var outConMgr = OutsideConnectionSettingsManager.instance;
            outConMgr.Init();
            outConMgr.SyncWithBuildingManager();

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

                SerializableDataExtension.instance.Loaded = false;

                if (m_IOCGameObject)
                    UnityEngine.Object.Destroy(m_IOCGameObject);

                OutsideConnectionSettingsManager.instance.Deinit();
            }
            finally
            {
                InGame = false;
            }
        }
    }
}
