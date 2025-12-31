// using HarmonyLib;
// using JetBrains.Annotations;
// using More_World_Locations_AIO.Managers;
//
// namespace More_World_Locations_AIO;
//
// [HarmonyPatch(typeof(WearNTear),nameof(WearNTear.RPC_Damage))]
// public class WearNTearPatch
// {
//     [UsedImplicitly]
//     private static bool Prefix(WearNTear __instance, HitData hit)
//     {
//         // patch to make wear n tear objects in our large location immune
//         foreach (Location? location in Location.s_allLocations)
//         {
//             if (!location.IsInside(__instance.transform.position, 50f, true)) continue;
//             if (Helpers.GetNormalizedName(location.name) != "MWL_Port_Location_Large") continue;
//             return false;
//         }
//         return true;
//     }
// }