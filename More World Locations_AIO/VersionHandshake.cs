using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;

namespace More_World_Locations_AIO
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    public static class RegisterAndCheckVersion
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            // Register version check call
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("Registering version RPC handler");
            peer.m_rpc.Register($"{More_World_Locations_AIOPlugin.ModName}_VersionCheck",
                new Action<ZRpc, ZPackage>(RpcHandlers.RPC_More_World_Locations_AIO_Version));

            // Make calls to check versions
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug("Invoking version check");
            ZPackage zpackage = new();
            zpackage.Write(More_World_Locations_AIOPlugin.ModVersion);
            peer.m_rpc.Invoke($"{More_World_Locations_AIOPlugin.ModName}_VersionCheck", zpackage);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    public static class VerifyClient
    {
        private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
        {
            if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc)) return true;
            // Disconnect peer if they didn't send mod version at all
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
            rpc.Invoke("Error", 3);
            return false; // Prevent calling underlying method
        }

        private static void Postfix(ZNet __instance)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                $"{More_World_Locations_AIOPlugin.ModName}RequestAdminSync",
                new ZPackage());
        }
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError))]
    public class ShowConnectionError
    {
        private static void Postfix(FejdStartup __instance)
        {
            if (__instance.m_connectionFailedPanel.activeSelf)
            {
                __instance.m_connectionFailedError.fontSizeMax = 25;
                __instance.m_connectionFailedError.fontSizeMin = 15;
                __instance.m_connectionFailedError.text += $"\n{More_World_Locations_AIOPlugin.ConnectionError}";
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    public static class RemoveDisconnectedPeerFromVerified
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            if (!__instance.IsServer()) return;
            // Remove peer from validated list
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
            _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
        }
    }

    public static class RpcHandlers
    {
        public static readonly List<ZRpc> ValidatedPeers = new();

        public static void RPC_More_World_Locations_AIO_Version(ZRpc rpc, ZPackage pkg)
        {
            string? version = pkg.ReadString();

            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"Version check, local: {More_World_Locations_AIOPlugin.ModVersion},  remote: {version}");
            if (version != More_World_Locations_AIOPlugin.ModVersion)
            {
                More_World_Locations_AIOPlugin.ConnectionError =
                    $"{More_World_Locations_AIOPlugin.ModName} Installed: {More_World_Locations_AIOPlugin.ModVersion}\n Needed: {version}";
                if (!ZNet.instance.IsServer()) return;
                // Different versions - force disconnect client from server
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                    $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
                rpc.Invoke("Error", 3);
            }
            else
            {
                if (!ZNet.instance.IsServer())
                {
                    // Enable mod on client if versions match
                    More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                        "Received same version from server!");
                }
                else
                {
                    // Add client to validated list
                    More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                        $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                    ValidatedPeers.Add(rpc);
                }
            }
        }
    }
}