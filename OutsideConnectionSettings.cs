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
        public List<string> RandomGenerationNames { get; set; } = new List<string>();
        public string SingleGenerationName { get; set; } = "";

        public string Name = "";

        public Building.Flags OriginalDirectionFlags;
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
                Utils.Log("No saved data found.");

            SerializableDataExtension.instance.EventSaveData += OnSaveData;
        }

        public void Deinit()
        {
            m_SettingsDict = null;

            SerializableDataExtension.instance.EventSaveData -= OnSaveData;
        }

        public void SyncWithBuildingManager()
        {
            var typeCount = new int[Utils.MaxEnumValue<TransportInfo.TransportType>() + 1];
            var oldSettingsDict = m_SettingsDict;
            m_SettingsDict = new Dictionary<ushort, OutsideConnectionSettings>();
            foreach (var buildingID in BuildingManager.instance.GetOutsideConnections())
            {
                OutsideConnectionSettings settings;
                var transportType = Utils.QueryTransportInfo(buildingID).m_transportType;
                if (oldSettingsDict.TryGetValue(buildingID, out settings))
                {
                    m_SettingsDict.Add(buildingID, settings);
                }
                else
                {
                    settings = new OutsideConnectionSettings();
                    
                    settings.Name = transportType.ToString() + "-Outside Connection " + (typeCount[(int)transportType] + 1);
                    var building = Utils.QueryBuilding(buildingID);
                    settings.OriginalDirectionFlags = building.m_flags & Building.Flags.IncomingOutgoing;
                    m_SettingsDict.Add(buildingID, settings);
                }
                ++typeCount[(int)transportType];
            }
        }

        private static void OnSaveData()
        {
            Utils.Log("Begin save data.");
            FastList<byte> buffer = new FastList<byte>();
            try
            {
                SerializableDataExtension.WriteUInt16(_dataVersion, buffer);
                SerializableDataExtension.WriteInt32(instance.SettingsDict.Count, buffer);
                foreach (var keyValue in instance.SettingsDict)
                {
                    SerializableDataExtension.WriteUInt16(keyValue.Key, buffer);
                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.NameMode, buffer);
                    SerializableDataExtension.WriteString(keyValue.Value.SingleGenerationName, buffer);

                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.RandomGenerationNames.Count, buffer);
                    foreach (var str in keyValue.Value.RandomGenerationNames)
                    {
                        SerializableDataExtension.WriteString(str, buffer);
                    }

                    SerializableDataExtension.WriteString(keyValue.Value.Name, buffer);
                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.OriginalDirectionFlags, buffer);
                }
                SerializableDataExtension.instance.SerializableData.SaveData(_dataID, buffer.ToArray());
                Utils.Log("End save data. Version: " + _dataVersion + " bytes written: " + buffer.m_size);
            }
            catch (Exception ex)
            {
                Utils.LogError("Error while saving data! " + ex.Message + " " + (object)ex.InnerException);
            }
        }

        private static bool TryLoadData(out Dictionary<ushort, OutsideConnectionSettings> settingsDict)
        {
            settingsDict = new Dictionary<ushort, OutsideConnectionSettings>();

            Utils.Log("Begin load data.");
            byte[] buffer = SerializableDataExtension.instance.SerializableData.LoadData(_dataID);
            if (buffer == null)
            {
                Utils.Log("No related data found.");
                return false;
            }

            int index = 0;
            try
            {
                var version = SerializableDataExtension.ReadUInt16(buffer, ref index);
                Utils.Log("Start deserialize data. Version: " + version + " bytes read: " + buffer.Length);

                switch (version)
                {
                    case 1:
                        return TryLoadData1(ref settingsDict, ref buffer, ref index);
                    default:
                        Utils.LogError("No protocol defined for version: " + version);
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError("Error while loading data! " + ex.Message + " " + (object)ex.InnerException);
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
                settings.SingleGenerationName = SerializableDataExtension.ReadString(buffer, ref index);

                var strCount = SerializableDataExtension.ReadUInt16(buffer, ref index);
                for (int j = 0; j < strCount; ++j)
                {
                    settings.RandomGenerationNames.Add(SerializableDataExtension.ReadString(buffer, ref index));
                }

                settings.Name = SerializableDataExtension.ReadString(buffer, ref index);
                settings.OriginalDirectionFlags = (Building.Flags)SerializableDataExtension.ReadUInt16(buffer, ref index);

                settingsDict.Add(buildingID, settings);
            }
            return true;
        }
    }
}
