using Harmony;
using ICities;
using ColossalFramework;
using System.Collections.Generic;

namespace ImprovedOutsideConnection.HarmonyPatches
{
    [HarmonyPatch(typeof(OutsideConnectionAI))]
    [HarmonyPatch("GenerateName")]
    //[HarmonyPatch(new Type[] { typeof(int) })]
    class OutsideConnectionGenerateName
    {
        private static Dictionary<ushort, string> m_OutsideConnectionNameDict = new Dictionary<ushort, string>();

        private static bool Prefix(ushort buildingID, ref string __result)
        {
            //var outsideConnections = BuildingManager.instance.GetOutsideConnections();

            if (!m_OutsideConnectionNameDict.TryGetValue(buildingID, out __result))
            {
                __result = "Test" + m_OutsideConnectionNameDict.Count;
                m_OutsideConnectionNameDict.Add(buildingID, __result);
            }
            return false;    // Well, we skip the original code, because it isn't necessary and this way we safe some performance. Are there any good reasons not to do?
        }
    }
}
