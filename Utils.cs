using System;
using System.Linq;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    public delegate void OutsideConnectionPropertyChanged<T>(ushort buildingID, T value);
    public delegate void DetailsOpenEventHandler(ushort buildingID);
    public static class Utils
    {
        public static TransportInfo QueryTransportInfo(ushort buildingID)
        {
            return QueryBuildingAI(buildingID)?.GetTransportLineInfo();
        }

        public static BuildingAI QueryBuildingAI(ushort buildingID)
        {
            return QueryBuilding(buildingID).Info.m_buildingAI;
        }

        public static Building QueryBuilding(ushort buildingID)
        {
            return BuildingManager.instance.m_buildings.m_buffer[buildingID];
        }

        public static T MaxEnumValue<E, T>()
        {
            return Enum.GetValues(typeof(E)).Cast<T>().Max();
        }

        public static int MaxEnumValue<E>()
        {
            return MaxEnumValue<E, int>();
        }

        public static T MinEnumValue<E, T>()
        {
            return Enum.GetValues(typeof(E)).Cast<T>().Min();
        }

        public static int MinEnumValue<E>()
        {
            return MinEnumValue<E, int>();
        }

        public static void Log(object message)
        {
            Debug.Log(Mod.ModName + ": " + message.ToString());
        }

        public static void LogError(object message)
        {
            Debug.LogError(Mod.ModName + ": " + message.ToString());
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(Mod.ModName + ": " + message.ToString());
        }
    }
}
