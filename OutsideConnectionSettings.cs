
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

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

        // Cache original direction flags; won't be serialized
        public Building.Flags OriginalDirectionFlags;
        public Building.Flags CurrentDirectionFlags;

        public int DummyTrafficFactor = -1;

        // When a custom building name is applied, the GenerateName function won't be called.
        // Let's just introduce a custom Name instead, so we don't have to patch the more central
        // GetBuildingName function and can simply rely on OutsideConnectionAI.GenerateName.
        public string Name = "";
    }

    class OutsideConnectionSettingsManager : Singleton<OutsideConnectionSettingsManager>
    {
        private static readonly string _dataID = "AOC_Data";
        private static readonly ushort _dataVersion = 2;

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
            var typeCount = new int[Utils.MaxEnumValue<TransferManager.TransferReason>() + 1];
            var oldSettingsDict = m_SettingsDict;
            m_SettingsDict = new Dictionary<ushort, OutsideConnectionSettings>();
            foreach (var buildingID in BuildingManager.instance.GetOutsideConnections())
            {
                var connectionAI = Utils.QueryBuildingAI(buildingID) as OutsideConnectionAI;
                if (connectionAI == null)
                    continue;

                var transferReason = connectionAI.m_dummyTrafficReason;

                OutsideConnectionSettings settings;
                if (oldSettingsDict.TryGetValue(buildingID, out settings))
                {
                    m_SettingsDict.Add(buildingID, settings);
                }
                else
                {
                    settings = new OutsideConnectionSettings();
                    settings.Name = Utils.GetNameForTransferReason(transferReason) + "-Outside Connection " + (typeCount[(int)transferReason] + 1);
                    var building = Utils.QueryBuilding(buildingID);
                    settings.CurrentDirectionFlags = building.m_flags & Building.Flags.IncomingOutgoing;
                    settings.OriginalDirectionFlags = settings.CurrentDirectionFlags;
                    settings.DummyTrafficFactor = connectionAI.m_dummyTrafficFactor;

                    m_SettingsDict.Add(buildingID, settings);
                }
                ++typeCount[(int)transferReason];
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
                    FastList<byte> elementBuffer = new FastList<byte>();
                    SerializableDataExtension.WriteUInt16(keyValue.Key, elementBuffer);
                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.NameMode, elementBuffer);
                    SerializableDataExtension.WriteString(keyValue.Value.SingleGenerationName, elementBuffer);

                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.RandomGenerationNames.Count, elementBuffer);
                    foreach (var str in keyValue.Value.RandomGenerationNames)
                    {
                        SerializableDataExtension.WriteString(str, elementBuffer);
                    }

                    SerializableDataExtension.WriteUInt16((ushort)keyValue.Value.CurrentDirectionFlags, elementBuffer);
                    SerializableDataExtension.WriteString(keyValue.Value.Name, elementBuffer);
                    SerializableDataExtension.WriteInt32(keyValue.Value.DummyTrafficFactor, elementBuffer);     // addition with version 2

                    SerializableDataExtension.WriteByteArray(elementBuffer, buffer);
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

                if (version >= 1)
                {
                    return TryLoadData1(version, ref settingsDict, ref buffer, ref index);
                }
                else
                    Utils.LogError("No protocol defined for version: " + version);
            }
            catch (Exception ex)
            {
                Utils.LogError("Error while loading data! " + ex.Message + " " + (object)ex.InnerException);
            }
            return false;
        }

        private static bool TryLoadData1(int version, ref Dictionary<ushort, OutsideConnectionSettings> settingsDict, ref byte[] buffer, ref int index)
        {
            var length = SerializableDataExtension.ReadInt32(buffer, ref index);
            for (int i = 0; i < length; ++i)
            {
                var curElementLength = SerializableDataExtension.ReadInt32(buffer, ref index);
                var indexBegin = index;

                var settings = new OutsideConnectionSettings();
                var buildingID = SerializableDataExtension.ReadUInt16(buffer, ref index);
                settings.NameMode = (OutsideConnectionSettings.NameModeType)SerializableDataExtension.ReadUInt16(buffer, ref index);
                settings.SingleGenerationName = SerializableDataExtension.ReadString(buffer, ref index);

                var strCount = SerializableDataExtension.ReadUInt16(buffer, ref index);
                for (int j = 0; j < strCount; ++j)
                {
                    settings.RandomGenerationNames.Add(SerializableDataExtension.ReadString(buffer, ref index));
                }

                settings.CurrentDirectionFlags = (Building.Flags)SerializableDataExtension.ReadUInt16(buffer, ref index) & Building.Flags.IncomingOutgoing;
                settings.Name = SerializableDataExtension.ReadString(buffer, ref index);

                var flags = Utils.QueryBuilding(buildingID).m_flags;
                settings.OriginalDirectionFlags = flags & Building.Flags.IncomingOutgoing;
                flags &= (~Building.Flags.IncomingOutgoing) | settings.CurrentDirectionFlags;
                BuildingManager.instance.m_buildings.m_buffer[buildingID].m_flags = flags;

                if (version >= 2)
                {
                    settings.DummyTrafficFactor = SerializableDataExtension.ReadInt32(buffer, ref index);
                }

                if (settingsDict.ContainsKey(buildingID))
                    Utils.LogWarning("Overrides existing outside connection with buildingID: " + buildingID);
                settingsDict[buildingID] = settings;
            }
            return true;
        }
    }
}
