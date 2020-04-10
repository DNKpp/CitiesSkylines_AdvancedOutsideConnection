
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Linq;
using UnityEngine;

namespace AdvancedOutsideConnection
{
    using fw = framework;

    public delegate void OutsideConnectionPropertyChanged<T>(ushort buildingID, T value);
    public delegate void DetailsOpenEventHandler(ushort buildingID);
    public static class Utils
    {
        public static int[] GetTouristFactorsFromOutsideConnection(ushort buildingID)
        {
            var connectionAI = QueryBuildingAI(buildingID) as OutsideConnectionAI;
            if (connectionAI)
                return new int[] { connectionAI.m_touristFactor0, connectionAI.m_touristFactor1, connectionAI.m_touristFactor2 };
            return null;
        }

        public static bool IsRoutesViewOn()
        {
            return InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes &&
                InfoManager.instance.CurrentSubMode == InfoManager.SubInfoMode.Default;
        }

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

        public static string GetSpriteNameForTransferReason(TransferManager.TransferReason reason)
        {
            switch (reason)
            {
                case TransferManager.TransferReason.DummyCar: return fw.CommonSprites.SubBarRoadsHighway.normal;
                case TransferManager.TransferReason.DummyPlane: return fw.CommonSprites.SubBarPublicTransportPlane.normal;
                case TransferManager.TransferReason.DummyTrain: return fw.CommonSprites.SubBarPublicTransportTrain.normal;
                case TransferManager.TransferReason.DummyShip: return fw.CommonSprites.SubBarPublicTransportShip.normal;
            }
            return "";
        }

        public static string GetNameForTransferReason(TransferManager.TransferReason reason)
        {
            switch (reason)
            {
                case TransferManager.TransferReason.DummyCar: return "Road";
                case TransferManager.TransferReason.DummyPlane: return "Plane";
                case TransferManager.TransferReason.DummyTrain: return "Train";
                case TransferManager.TransferReason.DummyShip: return "Ship";
            }
            return "Invalid";
        }

        public static string GetStringForDirectionFlag(Building.Flags directionFlags)
        {
            var flag = directionFlags & Building.Flags.IncomingOutgoing;
            if (flag == Building.Flags.IncomingOutgoing)
                return "In&Out";
            else if (flag == Building.Flags.Outgoing)   // It's from the buildings perspektive, not the cities.
                return "In";
            else if (flag == Building.Flags.Incoming)
                return "Out";
            return "None";
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
