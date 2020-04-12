
//          Copyright Dominic Koepke 2020 - 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          https://www.boost.org/LICENSE_1_0.txt)

using ColossalFramework.UI;
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
        public static void ModMaterialIfNecessary(ref TransferManager.TransferReason material, int[] ratios, TransferManager.TransferReason[] expectedMaterials)
        {
            var offerMaterial = material;
            var index = Array.FindIndex(expectedMaterials, row => row == offerMaterial);
            if (index != -1)
            {
                var rnd = SimulationManager.instance.m_randomizer.Int32(10000);
                int newMatIndex = 0;
                foreach (var ratio in ratios)
                {
                    if (rnd <= ratio)
                    {
                        break;
                    }
                    rnd -= ratio;
                    ++newMatIndex;
                }
                material = expectedMaterials[newMatIndex];
            }
        }

        public static void SetupSpriteForMaterial(UISprite sprite, TransferManager.TransferReason material)
        {
            string spriteName = "";
            string tooltip = "";
            switch (material)
            {   // extractor ressources both, vanilla and industries dlc
                case TransferManager.TransferReason.Ore:
                    spriteName = fw.CommonSprites.ResourceIconOre;
                    tooltip = "Vanilla and Industries DLC extracting resource ore.";
                    break;
                case TransferManager.TransferReason.Oil:
                    spriteName = fw.CommonSprites.ResourceIconOil;
                    tooltip = "Vanilla and Industries DLC extracting resource oil.";
                    break;
                case TransferManager.TransferReason.Grain:
                    spriteName = fw.CommonSprites.ResourceIconCrops;
                    tooltip = "Vanilla and Industries DLC extracting resource crops.";
                    break;
                case TransferManager.TransferReason.Logs:
                    spriteName = fw.CommonSprites.ResourceIconLogs;
                    tooltip = "Vanilla and Industries DLC extracting resource logs.";
                    break;

                // processing resource vanilla
                case TransferManager.TransferReason.Coal:
                    spriteName = fw.CommonSprites.IconPolicyOre.normal;
                    tooltip = "Vanilla final resource coal.";
                    break;
                case TransferManager.TransferReason.Petrol:
                    spriteName = fw.CommonSprites.IconPolicyOil.normal;
                    tooltip = "Vanilla final resource petrol.";
                    break;
                case TransferManager.TransferReason.Food:
                    spriteName = fw.CommonSprites.IconPolicyFarming.normal;
                    tooltip = "Vanilla final resource food.";
                    break;
                case TransferManager.TransferReason.Lumber:
                    spriteName = fw.CommonSprites.IconPolicyForest.normal;
                    tooltip = "Vanilla final resource lumber.";
                    break;
                case TransferManager.TransferReason.Goods:
                    tooltip = "Vanilla final resource goods.";
                    spriteName = fw.CommonSprites.IconOutsideConnections.normal;
                    break;

                // processing ressource industries dlc
                case TransferManager.TransferReason.AnimalProducts:
                    spriteName = fw.CommonSprites.ResourceIconAnimalProducts;
                    tooltip = "Industries DLC processing resource animal products.";
                    break;
                case TransferManager.TransferReason.Flours:
                    spriteName = fw.CommonSprites.ResourceIconFlours;
                    tooltip = "Industries DLC processing resource flours.";
                    break;
                case TransferManager.TransferReason.Paper:
                    spriteName = fw.CommonSprites.ResourceIconPaper;
                    tooltip = "Industries DLC processing resource paper.";
                    break;
                case TransferManager.TransferReason.PlanedTimber:
                    spriteName = fw.CommonSprites.ResourceIconPlanedTimber;
                    tooltip = "Industries DLC processing resource planed timber.";
                    break;
                case TransferManager.TransferReason.Petroleum:
                    spriteName = fw.CommonSprites.ResourceIconPetroleum;
                    tooltip = "Industries DLC processing resource petroleum.";
                    break;
                case TransferManager.TransferReason.Plastics:
                    spriteName = fw.CommonSprites.ResourceIconPlastics;
                    tooltip = "Industries DLC processing resource plastics.";
                    break;
                case TransferManager.TransferReason.Glass:
                    spriteName = fw.CommonSprites.ResourceIconGlass;
                    tooltip = "Industries DLC processing resource glass.";
                    break;
                case TransferManager.TransferReason.Metals:
                    spriteName = fw.CommonSprites.ResourceIconMetal;
                    tooltip = "Industries DLC processing resource metals.";
                    break;
                case TransferManager.TransferReason.LuxuryProducts:
                    tooltip = "Industries DLC final resource luxury goods.";
                    spriteName = fw.CommonSprites.ResourceIconLuxuryProducts;
                    break;
                case TransferManager.TransferReason.UnsortedMail:
                    tooltip = "Industries DLC processing resource unsorted mail.";
                    spriteName = fw.CommonSprites.InfoIconPost;
                    break;
                case TransferManager.TransferReason.SortedMail:
                    tooltip = "Industries DLC final resource sorted mail.";
                    spriteName = fw.CommonSprites.InfoIconPost;
                    break;

                // sunset harbour dlc
                case TransferManager.TransferReason.Fish:
                    spriteName = fw.CommonSprites.InfoIconFishing;
                    tooltip = "Sunset harbour DLC resource fish.";
                    break;
            }
            sprite.spriteName = spriteName;
            sprite.tooltip = tooltip;
        }

        public static int[] GetTouristFactorsFromOutsideConnection(ushort buildingID)
        {
            var connectionAI = QueryBuildingAI(buildingID) as OutsideConnectionAI;
            if (connectionAI)
                return new int[] { connectionAI.m_touristFactor0, connectionAI.m_touristFactor1, connectionAI.m_touristFactor2 };
            return null;
        }

        public static int[] GetDefaultImportResourceRatio()
        {
            return GetDefaultRatioArray(10000, ImportResources.Length);
        }

        public static int[] GetDefaultExportResourceRatio()
        {
            return GetDefaultRatioArray(10000, ExportResources.Length);
        }

        public static int[] GetDefaultRatioArray(int totalValue, int length)
        {
            var ratios = new int[length];
            int left = totalValue;
            for (int i = 0; i < length; ++i)
            {
                ratios[i] = Mathf.RoundToInt((float)left / (length - i));
                left -= ratios[i];
            }
            return ratios;
        }

        public static bool ApplyNewImportResourceRatio(ref int[] ratios, int newValue, TransferManager.TransferReason resource)
        {
            var index = Array.FindIndex(ImportResources, row => row == resource);
            return ApplyNewRatio(ref ratios, newValue, index);
        }

        public static bool ApplyNewExportResourceRatio(ref int[] ratios, int newValue, TransferManager.TransferReason resource)
        {
            var index = Array.FindIndex(ExportResources, row => row == resource);
            return ApplyNewRatio(ref ratios, newValue, index);
        }

        public static bool ApplyNewRatio(ref int[] ratios, int newValue, int index)
        {
            if (index < 0 || ratios.Length <= index || newValue < 0 || 10000 < newValue)
                return false;

            var valueLeft = 10000 - newValue;
            var prevIndexValue = ratios[index];
            if (prevIndexValue != 10000)
            {
                for (int i = 0; i < ratios.Length; ++i)
                {
                    if (i != index)
                    {
                        var prevValue = ratios[i];
                        if (prevIndexValue != 10000)
                            ratios[i] = Mathf.RoundToInt((float)ratios[i] * valueLeft / (10000 - prevIndexValue));
                        else
                            ratios[i] = 0;
                        valueLeft -= ratios[i];
                        prevIndexValue = Mathf.Min(10000, prevValue + prevIndexValue);
                    }
                    else
                        ratios[i] = newValue;
                }
            }
            else
            {
                int ratiosLeft = ratios.Length - 1;
                for (int i = 0; i < ratios.Length; ++i)
                {
                    if (i != index)
                    {
                        ratios[i] = Mathf.Min(valueLeft, Mathf.RoundToInt((float)valueLeft / ratiosLeft--));
                        valueLeft -= ratios[i];
                    }
                    else
                        ratios[i] = newValue;
                }
            }
            return true;
        }

        public static readonly TransferManager.TransferReason[] ImportResources =
        {
                TransferManager.TransferReason.Oil,
                TransferManager.TransferReason.Ore,
                TransferManager.TransferReason.Logs,
                TransferManager.TransferReason.Grain,
                TransferManager.TransferReason.Coal,
                TransferManager.TransferReason.Petrol,
                TransferManager.TransferReason.Food,
                TransferManager.TransferReason.Lumber,
                TransferManager.TransferReason.Goods,
                TransferManager.TransferReason.UnsortedMail,
                TransferManager.TransferReason.SortedMail
        };

        public static readonly TransferManager.TransferReason[] ExportResources =
        {
                TransferManager.TransferReason.Oil,
                TransferManager.TransferReason.Ore,
                TransferManager.TransferReason.Logs,
                TransferManager.TransferReason.Grain,
                TransferManager.TransferReason.Coal,
                TransferManager.TransferReason.Petrol,
                TransferManager.TransferReason.Food,
                TransferManager.TransferReason.Lumber,
                TransferManager.TransferReason.Goods,
                TransferManager.TransferReason.UnsortedMail,
                TransferManager.TransferReason.SortedMail,

                TransferManager.TransferReason.Glass,
                TransferManager.TransferReason.Metals,
                TransferManager.TransferReason.Petroleum,
                TransferManager.TransferReason.Plastics,
                TransferManager.TransferReason.AnimalProducts,
                TransferManager.TransferReason.Flours,
                TransferManager.TransferReason.Paper,
                TransferManager.TransferReason.PlanedTimber,
                TransferManager.TransferReason.LuxuryProducts,
                TransferManager.TransferReason.Fish
        };

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
