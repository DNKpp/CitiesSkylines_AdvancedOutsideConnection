using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImprovedOutsideConnection
{
    class OutsideConnectionSettings
    {
        public enum NameModeType
        {
            Vanilla         = 0,
            CustomFixed,
            CustomRandom
        }
        public NameModeType NameMode { get; set; }
        public TransportInfo.TransportType Type { get; set; }
    }

    class OutsideConnectionSettingsManager : Singleton<OutsideConnectionSettingsManager>
    {
        private static Dictionary<ushort, OutsideConnectionSettings> m_SettingsDict = new Dictionary<ushort, OutsideConnectionSettings>();

        public OutsideConnectionSettings GetSettings(ushort buildingID)
        {
            OutsideConnectionSettings settings;
            m_SettingsDict.TryGetValue(buildingID, out settings);
            return settings;
        }

        public void SyncWithBuildingManager()
        {
            var buildingMgr = BuildingManager.instance;
            foreach (var buildingID in BuildingManager.instance.GetOutsideConnections())
            {
                var oldSettingsDict = m_SettingsDict;
                m_SettingsDict = null;
                OutsideConnectionSettings settings;
                if (oldSettingsDict.TryGetValue(buildingID, out settings))
                {
                    m_SettingsDict.Add(buildingID, settings);
                }
                else
                {
                    var buildingAI = buildingMgr.m_buildings.m_buffer[buildingID].Info.m_buildingAI;
                    var transportInfo = buildingAI.GetTransportLineInfo();
                    settings = new OutsideConnectionSettings();
                    settings.Type = transportInfo.m_transportType;
                    m_SettingsDict.Add(buildingID, settings);
                }
            }
        }
    }
}
