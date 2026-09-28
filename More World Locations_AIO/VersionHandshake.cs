using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using More_World_Locations_AIO.ServerOnly;

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
            zpackage.Write(ServerOnlyHandshake.Announce(
                More_World_Locations_AIOPlugin.ModVersion, ServerOnlyMode.Enabled));
            peer.m_rpc.Invoke($"{More_World_Locations_AIOPlugin.ModName}_VersionCheck", zpackage);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    public static class VerifyClient
    {
        private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
        {
            if (!__instance.IsServer()) return true;

            // A peer's version answer is queued on its socket before its PeerInfo,
            // so by now the server has recorded a match, a mismatch, or nothing.
            // Nothing means no MWL on the client. The decision is made here for
            // every peer, validated ones included: a "validated, so let it in"
            // shortcut ahead of this would walk a modded client straight past
            // server-only mode's refusal of exactly that client.
            PeerAdmission.Verdict verdict = PeerAdmission.DecideFor(
                validated: RpcHandlers.ValidatedPeers.Contains(rpc),
                refused: RpcHandlers.RefusedPeers.Contains(rpc));

            if (PeerAdmission.Admits(verdict, ServerOnlyMode.Enabled))
            {
                if (verdict == PeerAdmission.Verdict.WithoutMod)
                {
                    More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                        $"A peer without {More_World_Locations_AIOPlugin.ModName} joined: it receives the approved locations as vanilla objects");
                }
                return true;
            }

            // Refused here as well as when its version answer arrived, so
            // admission never depends on the client acting on the error it was
            // sent.
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) refused at PeerInfo: "
                + PeerAdmission.RefusalReason(verdict, ServerOnlyMode.Enabled));
            rpc.Invoke("Error", (int)ZNet.ConnectionStatus.ErrorVersion);
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
            _ = RpcHandlers.RefusedPeers.Remove(peer.m_rpc);
        }
    }

    public static class RpcHandlers
    {
        public static readonly List<ZRpc> ValidatedPeers = new();

        /// <summary>Peers that answered the version check with another version.</summary>
        public static readonly List<ZRpc> RefusedPeers = new();

        public static void RPC_More_World_Locations_AIO_Version(ZRpc rpc, ZPackage pkg)
        {
            string? version = pkg.ReadString();

            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                $"Version check, local: {More_World_Locations_AIOPlugin.ModVersion},  remote: {version}");

            bool isServer = ZNet.instance.IsServer();

            // On a client: a server-only server announces itself, so say what to
            // do about it rather than leaving the player a version number that
            // matches their own.
            if (!isServer && ServerOnlyHandshake.AnnouncesServerOnly(version))
            {
                More_World_Locations_AIOPlugin.ConnectionError =
                    ServerOnlyHandshake.ClientMessage(More_World_Locations_AIOPlugin.ModName, version);
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                    "This server runs " + More_World_Locations_AIOPlugin.ModName
                    + " in server-only mode and is for clients without the mod");
                return;
            }

            if (version != More_World_Locations_AIOPlugin.ModVersion)
            {
                More_World_Locations_AIOPlugin.ConnectionError =
                    $"{More_World_Locations_AIOPlugin.ModName} Installed: {More_World_Locations_AIOPlugin.ModVersion}\n Needed: {version}";
                if (!isServer) return;
                // Different versions - force disconnect client from server
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                    $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
                if (!RefusedPeers.Contains(rpc)) RefusedPeers.Add(rpc);
                rpc.Invoke("Error", (int)ZNet.ConnectionStatus.ErrorVersion);
                return;
            }

            if (!isServer)
            {
                // Enable mod on client if versions match
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    "Received same version from server!");
                return;
            }

            // On the server the peer has now identified itself as having the mod.
            // It is recorded either way; whether that admits it is
            // PeerAdmission's decision and depends on the mode.
            ValidatedPeers.Add(rpc);
            PeerAdmission.Verdict verdict = PeerAdmission.Decide(
                answeredVersionCheck: true, versionMatched: true);
            if (PeerAdmission.Admits(verdict, ServerOnlyMode.Enabled))
            {
                More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogInfo(
                    $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                return;
            }

            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) refused: "
                + PeerAdmission.RefusalReason(verdict, ServerOnlyMode.Enabled));
            rpc.Invoke("Error", (int)ZNet.ConnectionStatus.ErrorVersion);
        }
    }
}