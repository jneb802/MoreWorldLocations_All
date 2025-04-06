using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;

namespace Forbidden_Catacombs
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    public static class RegisterAndCheckVersion
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            // Register version check call
            Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogDebug("Registering version RPC handler");
            peer.m_rpc.Register($"{Forbidden_CatacombsPlugin.ModName}_VersionCheck",
                new Action<ZRpc, ZPackage>(RpcHandlers.RPC_Dungeon_Castle_Version));

            // Make calls to check versions
            Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogDebug("Invoking version check");
            ZPackage zpackage = new();
            zpackage.Write(Forbidden_CatacombsPlugin.ModVersion);
            peer.m_rpc.Invoke($"{Forbidden_CatacombsPlugin.ModName}_VersionCheck", zpackage);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    public static class VerifyClient
    {
        private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
        {
            if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc)) return true;
            // Disconnect peer if they didn't send mod version at all
            Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
            rpc.Invoke("Error", 3);
            return false; // Prevent calling underlying method
        }

        private static void Postfix(ZNet __instance)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                $"{Forbidden_CatacombsPlugin.ModName}RequestAdminSync",
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
                __instance.m_connectionFailedError.text += $"\n{Forbidden_CatacombsPlugin.ConnectionError}";
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
            Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogInfo(
                $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
            _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
        }
    }

    public static class RpcHandlers
    {
        public static readonly List<ZRpc> ValidatedPeers = new();

        public static void RPC_Dungeon_Castle_Version(ZRpc rpc, ZPackage pkg)
        {
            string? version = pkg.ReadString();

            Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogInfo(
                $"Version check, local: {Forbidden_CatacombsPlugin.ModVersion},  remote: {version}");
            if (version != Forbidden_CatacombsPlugin.ModVersion)
            {
                Forbidden_CatacombsPlugin.ConnectionError =
                    $"{Forbidden_CatacombsPlugin.ModName} Installed: {Forbidden_CatacombsPlugin.ModVersion}\n Needed: {version}";
                if (!ZNet.instance.IsServer()) return;
                // Different versions - force disconnect client from server
                Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogWarning(
                    $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
                rpc.Invoke("Error", 3);
            }
            else
            {
                if (!ZNet.instance.IsServer())
                {
                    // Enable mod on client if versions match
                    Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogInfo("Received same version from server!");
                }
                else
                {
                    // Add client to validated list
                    Forbidden_CatacombsPlugin.Dungeon_CastleLogger.LogInfo(
                        $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                    ValidatedPeers.Add(rpc);
                }
            }
        }
    }
}