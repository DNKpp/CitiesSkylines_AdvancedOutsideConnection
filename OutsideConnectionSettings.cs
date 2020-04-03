using ColossalFramework;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;

namespace ImprovedOutsideConnection
{
    class OutsideConnectionSettings
    {
        public enum NameModeType
        {
            [Description("Vanilla")]
            Vanilla         = 0,
            [Description("CustomSingle")]
            CustomSingle,
            [Description("CustomRandom")]
            CustomRandom
        }
        public NameModeType NameMode { get; set; }
        public TransportInfo.TransportType Type { get; set; }
        public string NameText { get; set; } = new string(' ', 0);
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
    }

    class OutsideConnectionSettingsManager : Singleton<OutsideConnectionSettingsManager>
    {
        public Dictionary<ushort, OutsideConnectionSettings> m_SettingsDict { get; internal set; } = new Dictionary<ushort, OutsideConnectionSettings>();

        public void SyncWithBuildingManager()
        {
            var buildingMgr = BuildingManager.instance;
            var oldSettingsDict = m_SettingsDict;
            m_SettingsDict = new Dictionary<ushort, OutsideConnectionSettings>();
            foreach (var buildingID in BuildingManager.instance.GetOutsideConnections())
            {
                OutsideConnectionSettings settings;
                if (oldSettingsDict.TryGetValue(buildingID, out settings))
                {
                    m_SettingsDict.Add(buildingID, settings);
                }
                else
                {
                    var building = buildingMgr.m_buildings.m_buffer[buildingID];
                    var buildingAI = building.Info.m_buildingAI;
                    var transportInfo = buildingAI.GetTransportLineInfo();
                    settings = new OutsideConnectionSettings();
                    settings.Type = transportInfo.m_transportType;
                    settings.Position = building.m_position;
                    m_SettingsDict.Add(buildingID, settings);
                }
            }
        }

        public KeyValuePair<ushort, OutsideConnectionSettings>[] GetSettingsAsArray()
        {
            var settingsArr = new KeyValuePair<ushort, OutsideConnectionSettings>[m_SettingsDict.Count];
            int i = 0;
            foreach (var el in m_SettingsDict)
            {
                settingsArr[i++] = el;
            }
            Array.Sort(settingsArr, (lhs, rhs) =>
            {
                return (int)lhs.Value.Type - (int)rhs.Value.Type;
            });
            return settingsArr;
        }
    }
}
