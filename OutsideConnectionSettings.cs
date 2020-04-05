using ColossalFramework;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;

namespace AdvancedOutsideConnection
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
        public List<string> NameText { get; set; } = new List<string>();

        public string Name = "";
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
    }

    class OutsideConnectionSettingsManager : Singleton<OutsideConnectionSettingsManager>
    {
        private static readonly string _dataID = "AOC_Data";
        private static readonly ushort _dataVersion = 1;

        private Dictionary<ushort, OutsideConnectionSettings> m_SettingsDict = null;

        public Dictionary<ushort, OutsideConnectionSettings> SettingsDict
        {
            get { return m_SettingsDict; }
            set { this.m_SettingsDict = value; }
        }

        protected OutsideConnectionSettingsManager()
        {
        }

        public void Init()
        {
            if (!TryLoadData(out m_SettingsDict))
                Utils.Log("AdvancedOutsideConnection: No saved data found.");

            SerializableDataExtension.instance.EventSaveData += new SerializableDataExtension.SaveDataEventHandler(OutsideConnectionSettingsManager.OnSaveData);
        }

        public void Deinit()
        {
            m_SettingsDict = null;

            SerializableDataExtension.instance.EventSaveData -= new SerializableDataExtension.SaveDataEventHandler(OutsideConnectionSettingsManager.OnSaveData);
        }

        public void SyncWithBuildingManager()
        {
            var typeCount = new int[Utils.MaxEnumValue<TransportInfo.TransportType>() + 1];
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
                    settings = new OutsideConnectionSettings();
                    GetInfoFromOutsideConnection(ref settings, buildingID);
                    settings.Name = settings.Type.ToString() + "-Outside Connection " + (typeCount[(int)settings.Type] + 1);
                    m_SettingsDict.Add(buildingID, settings);
                }
                ++typeCount[(int)settings.Type];
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

        private static void GetInfoFromOutsideConnection(ref OutsideConnectionSettings settings, ushort buildingID)
        {
            var buildingMgr = BuildingManager.instance;
            var building = buildingMgr.m_buildings.m_buffer[buildingID];
            var buildingAI = building.Info.m_buildingAI;
            var transportInfo = buildingAI.GetTransportLineInfo();
            settings.Type = transportInfo.m_transportType;
            settings.Position = building.m_position;
        }

        static private void OnSaveData()
        {
            Utils.Log("AdvancedOutsideConnection: Begin save data.");
            FastList<byte> buffer = new FastList<byte>();
            try
            {
                SerializableDataExtension.WriteUInt16(_dataVersion, buffer);
                SerializableDataExtension.WriteInt32(instance.SettingsDict.Count, buffer);
                foreach (var keyValue in instance.SettingsDict)
                {
                    SerializableDataExtension.WriteUInt16(keyValue.Key, buffer);
                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.NameMode, buffer);

                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.NameText.Count, buffer);
                    foreach (var str in keyValue.Value.NameText)
                    {
                        SerializableDataExtension.WriteString(str, buffer);
                    }

                    SerializableDataExtension.WriteString(keyValue.Value.Name, buffer);
                }
                SerializableDataExtension.instance.SerializableData.SaveData(_dataID, buffer.ToArray());
                Utils.Log("AdvancedOutsideConnection: Save data successful.");
            }
            catch (Exception ex)
            {
                Utils.LogError("AdvancedOutsideConnection: Error while saving data! " + ex.Message + " " + (object)ex.InnerException);
            }
        }

        private static bool TryLoadData(out Dictionary<ushort, OutsideConnectionSettings> settingsDict)
        {
            settingsDict = new Dictionary<ushort, OutsideConnectionSettings>();

            Utils.Log("AdvancedOutsideConnection: Begin load data.");
            byte[] buffer = SerializableDataExtension.instance.SerializableData.LoadData(_dataID);
            if (buffer == null)
            {
                Utils.Log("AdvancedOutsideConnection: No related data found.");
                return false;
            }

            int index = 0;
            try
            {
                var version = SerializableDataExtension.ReadUInt16(buffer, ref index);
                Utils.Log("AdvancedOutsideConnection: Start deserialize data version: " + version);

                switch (version)
                {
                    case 1:
                        return TryLoadData1(ref settingsDict, ref buffer, ref index);
                    default:
                        Utils.LogError("AdvancedOutsideConnection: No protocol defined for version: " + version);
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError("AdvancedOutsideConnection: Error while loading data! " + ex.Message + " " + (object)ex.InnerException);
            }
            return false;
        }

        private static bool TryLoadData1(ref Dictionary<ushort, OutsideConnectionSettings> settingsDict, ref byte[] buffer, ref int index)
        {
            var length = SerializableDataExtension.ReadInt32(buffer, ref index);
            for (int i = 0; i < length; ++i)
            {
                var settings = new OutsideConnectionSettings();
                var buildingID = SerializableDataExtension.ReadUInt16(buffer, ref index);
                settings.NameMode = (OutsideConnectionSettings.NameModeType)SerializableDataExtension.ReadUInt16(buffer, ref index);

                var strCount = SerializableDataExtension.ReadUInt16(buffer, ref index);
                for (int j = 0; j < strCount; ++j)
                {
                    settings.NameText.Add(SerializableDataExtension.ReadString(buffer, ref index));
                }

                settings.Name = SerializableDataExtension.ReadString(buffer, ref index);

                GetInfoFromOutsideConnection(ref settings, buildingID);
                settingsDict.Add(buildingID, settings);
            }
            return true;
        }
    }
}
