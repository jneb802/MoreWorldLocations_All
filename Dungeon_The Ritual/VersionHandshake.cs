using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;

namespace Dungeon_The_Ritual
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    public static class RegisterAndCheckVersion
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            // Register version check call
            Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogDebug("Registering version RPC handler");
            peer.m_rpc.Register($"{Dungeon_The_RitualPlugin.ModName}_VersionCheck",
                new Action<ZRpc, ZPackage>(RpcHandlers.RPC_Dungeon_The_Ritual_Version));

            // Make calls to check versions
            Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogDebug("Invoking version check");
            ZPackage zpackage = new();
            zpackage.Write(Dungeon_The_RitualPlugin.ModVersion);
            peer.m_rpc.Invoke($"{Dungeon_The_RitualPlugin.ModName}_VersionCheck", zpackage);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    public static class VerifyClient
    {
        private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
        {
            if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc)) return true;
            // Disconnect peer if they didn't send mod version at all
            Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
            rpc.Invoke("Error", 3);
            return false; // Prevent calling underlying method
        }

        private static void Postfix(ZNet __instance)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                $"{Dungeon_The_RitualPlugin.ModName}RequestAdminSync",
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
                __instance.m_connectionFailedError.text += "\n" + Dungeon_The_RitualPlugin.ConnectionError;
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
            Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogInfo(
                $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
            _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
        }
    }

    public static class RpcHandlers
    {
        public static readonly List<ZRpc> ValidatedPeers = new();

        public static void RPC_Dungeon_The_Ritual_Version(ZRpc rpc, ZPackage pkg)
        {
            string? version = pkg.ReadString();

            Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogInfo("Version check, local: " +
                                                                      Dungeon_The_RitualPlugin.ModVersion +
                                                                      ",  remote: " + version);
            if (version != Dungeon_The_RitualPlugin.ModVersion)
            {
                Dungeon_The_RitualPlugin.ConnectionError =
                    $"{Dungeon_The_RitualPlugin.ModName} Installed: {Dungeon_The_RitualPlugin.ModVersion}\n Needed: {version}";
                if (!ZNet.instance.IsServer()) return;
                // Different versions - force disconnect client from server
                Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogWarning(
                    $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
                rpc.Invoke("Error", 3);
            }
            else
            {
                if (!ZNet.instance.IsServer())
                {
                    // Enable mod on client if versions match
                    Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogInfo(
                        "Received same version from server!");
                }
                else
                {
                    // Add client to validated list
                    Dungeon_The_RitualPlugin.Dungeon_The_RitualLogger.LogInfo(
                        $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                    ValidatedPeers.Add(rpc);
                }
            }
        }
    }
}