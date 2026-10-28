using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using More_World_Locations_AIO.Traders;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO;

/// <summary>
/// The teleport picker: the vanilla large map with every discovered port pinned and labeled, a
/// sidebar listing favorites, hints in the map's own hint bars, and full mouse and gamepad control. Opened from the port UI's
/// Teleport tab; closing the map (B / Escape / map key) returns to the port UI.
///
/// Mouse: left click a port to teleport, middle click to toggle it as a favorite, click a sidebar
/// entry to teleport there. Every teleport goes through a yes/no confirmation showing the cost.
/// Gamepad: the map pans under the center crosshair. A teleports to the port under the crosshair,
/// Y toggles it as a favorite, right stick up/down walks the favorites sidebar (the map follows),
/// LB/RB cycles through every known port, B goes back.
/// Anything aimed away from a port keeps its vanilla meaning, so the player's own pins can still
/// be crossed off, removed, added and filtered while picking; only the port pins are protected.
/// </summary>
public static class PortMap
{
    public static ConfigEntry<PortInit.Toggle>? HidePinsOnPortMap;

    public static bool IsOpen => s_open;

    private static bool s_open;
    private static Port? s_origin;
    private static int s_pinType = -1;
    private static readonly List<PortEntry> s_entries = new();
    private static readonly List<PortEntry> s_favorites = new();
    private static bool[]? s_savedIconFilters;
    private static Sidebar? s_sidebar;
    private static PortEntry? s_hovered;
    private static int s_favoriteIndex = -1;
    private static int s_cycleIndex = -1;

    private static readonly Color HighlightColor = new(1f, 0.85f, 0.3f);

    private class PortEntry
    {
        public Port.PortInfo Info = null!;
        public Minimap.PinData Pin = null!;
        public bool Favorite;
        public bool Adopted; // the location pin the mod already shows for this port, labeled for the picker
    }

    // ------------------------------------------------------------------ open / close

    public static void Open(Port origin)
    {
        if (Minimap.instance == null || Player.m_localPlayer == null) return;
        if (s_open) Close(false);

        List<Port.PortInfo> ports = CollectPorts(origin);
        if (ports.Count == 0)
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, LocalKeys.NoKnownPorts);
            return;
        }

        PortUI.instance?.Hide();

        // No-map worlds refuse the large map; the picker is the one place it has to open.
        bool noMap = Game.m_noMap;
        Game.m_noMap = false;
        Minimap.instance.ShowPointOnMap(origin.transform.position);
        Game.m_noMap = noMap;
        if (Minimap.instance.m_mode != Minimap.MapMode.Large) return;

        s_origin = origin;
        s_open = true;
        s_hovered = null;
        s_favoriteIndex = -1;
        s_cycleIndex = -1;

        AddPins(ports);
        ApplyIconFilters();
        MapHints.Show(true);
        EnsureSidebar().Show();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private static void Close(bool returnToPortUi)
    {
        if (!s_open) return;
        s_open = false;

        if (Minimap.instance != null)
        {
            foreach (PortEntry entry in s_entries)
            {
                if (entry.Adopted) ReleaseLocationPin(entry.Pin);
                else Minimap.instance.RemovePin(entry.Pin);
            }
        }
        s_entries.Clear();
        if (ZInput.IsGamepadActive())
        {
            // Whatever closed the picker (A on the confirmation, B to back out) must not also be
            // read as jump or use by the player controller right after.
            ControllerHints.Consume("JoyButtonA");
            ControllerHints.Consume("JoyButtonB");
        }
        s_favorites.Clear();
        s_hovered = null;
        s_favoriteIndex = -1;
        s_cycleIndex = -1;
        s_panTarget = null;
        RestoreIconFilters();
        MapHints.Show(false);
        s_sidebar?.Hide();

        Port? origin = s_origin;
        s_origin = null;
        if (!returnToPortUi || origin == null || PortUI.instance == null) return;
        if (origin.m_view == null || !origin.m_view.IsValid()) return;
        if (Player.m_localPlayer == null || Player.m_localPlayer.IsDead()) return;
        PortUI.instance.Show(origin);
    }

    private static List<Port.PortInfo> CollectPorts(Port origin)
    {
        List<Port.PortInfo> ports = new();
        if (Player.m_localPlayer == null) return ports;
        ZDO? originZdo = origin.m_view != null && origin.m_view.IsValid() ? origin.m_view.GetZDO() : null;
        foreach (ZDO port in ShipmentManager.GetPorts())
        {
            if (port == originZdo) continue;
            Port.PortInfo info = new Port.PortInfo(port);
            if (string.IsNullOrEmpty(info.portID.Name)) continue;
            if (!Player.m_localPlayer.IsKnownPort(info.portID)) continue;
            ports.Add(info);
        }
        return ports;
    }

    // Discovered port locations already carry a large anchor pin (a vanilla location pin with the
    // mod's icon). Labeling that pin is cleaner than stacking a second one on top of it; a port
    // without one gets its own pin of the same size.
    private static void AddPins(List<Port.PortInfo> ports)
    {
        Minimap map = Minimap.instance;
        Minimap.PinType type = s_pinType >= 0 ? (Minimap.PinType)s_pinType : Minimap.PinType.Icon4;
        HashSet<Minimap.PinData> taken = new();
        foreach (Port.PortInfo info in ports)
        {
            Minimap.PinData? pin = FindLocationPin(map, info.position, taken);
            bool adopted = pin != null;
            if (pin == null)
            {
                pin = map.AddPin(info.position, type, info.portID.Name, false, false);
                pin.m_doubleSize = true;
            }
            else
            {
                taken.Add(pin);
                pin.m_name = info.portID.Name;
                pin.m_NamePinData = new Minimap.PinNameData(pin);
                map.m_pinUpdateRequired = true;
            }
            s_entries.Add(new PortEntry { Info = info, Pin = pin, Adopted = adopted });
        }
        s_entries.Sort((a, b) => string.Compare(a.Info.portID.Name, b.Info.portID.Name, StringComparison.OrdinalIgnoreCase));
        RefreshFavorites();
    }

    private const float LocationPinRadius = 64f;

    private static Minimap.PinData? FindLocationPin(Minimap map, Vector3 position, HashSet<Minimap.PinData> taken)
    {
        Sprite? anchor = MinimapTraderIcons.achorSprite;
        if (anchor == null) return null;
        Minimap.PinData? best = null;
        float bestDistance = LocationPinRadius;
        foreach (Minimap.PinData pin in map.m_locationPins.Values)
        {
            if (pin.m_icon != anchor || taken.Contains(pin)) continue;
            float distance = global::Utils.DistanceXZ(pin.m_pos, position);
            if (distance >= bestDistance) continue;
            best = pin;
            bestDistance = distance;
        }
        return best;
    }

    private static void ReleaseLocationPin(Minimap.PinData pin)
    {
        if (pin.m_NamePinData != null && pin.m_NamePinData.PinNameGameObject != null) pin.m_NamePinData.DestroyMapMarker();
        pin.m_NamePinData = null;
        pin.m_name = "";
        if (pin.m_iconElement != null) pin.m_iconElement.color = Color.white;
    }

    private static void RefreshFavorites()
    {
        HashSet<string> favorites = Player.m_localPlayer != null ? Player.m_localPlayer.GetFavoritePorts() : new HashSet<string>();
        s_favorites.Clear();
        foreach (PortEntry entry in s_entries)
        {
            entry.Favorite = favorites.Contains(entry.Info.portID.GUID);
            if (entry.Favorite) s_favorites.Add(entry);
        }
        if (s_favoriteIndex >= s_favorites.Count) s_favoriteIndex = s_favorites.Count - 1;
        s_sidebar?.RefreshFavorites();
    }

    // The port map is a picker: player-placed pins only get in the way of the port pins, so they
    // are filtered off while it is open and restored afterwards. Locations, players, bosses and
    // the like stay visible so the player can still orient themselves.
    private static readonly Minimap.PinType[] HiddenWhilePicking =
    {
        Minimap.PinType.Icon0, Minimap.PinType.Icon1, Minimap.PinType.Icon2, Minimap.PinType.Icon3, Minimap.PinType.Icon4,
    };

    private static void ApplyIconFilters()
    {
        if (HidePinsOnPortMap?.Value != PortInit.Toggle.On || Minimap.instance == null) return;
        bool[] filters = Minimap.instance.m_visibleIconTypes;
        s_savedIconFilters = new bool[filters.Length];
        Array.Copy(filters, s_savedIconFilters, filters.Length);
        foreach (Minimap.PinType hidden in HiddenWhilePicking)
        {
            if ((int)hidden < filters.Length) filters[(int)hidden] = false;
        }
        Minimap.instance.m_pinUpdateRequired = true;
    }

    private static void RestoreIconFilters()
    {
        if (s_savedIconFilters == null || Minimap.instance == null) return;
        bool[] filters = Minimap.instance.m_visibleIconTypes;
        // Only the types the picker hid go back; filters the player toggled meanwhile stay as set.
        foreach (Minimap.PinType hidden in HiddenWhilePicking)
        {
            int index = (int)hidden;
            if (index < filters.Length && index < s_savedIconFilters.Length && !filters[index] && s_savedIconFilters[index]) filters[index] = true;
        }
        s_savedIconFilters = null;
        Minimap.instance.m_pinUpdateRequired = true;
    }

    private static Sidebar EnsureSidebar()
    {
        if (s_sidebar == null || !s_sidebar.IsAlive) s_sidebar = new Sidebar(Minimap.instance.m_largeRoot.transform);
        return s_sidebar;
    }

    // ------------------------------------------------------------------ actions

    private static PortEntry? CurrentTarget()
    {
        if (s_favoriteIndex >= 0 && s_favoriteIndex < s_favorites.Count) return s_favorites[s_favoriteIndex];
        return s_hovered;
    }

    /// <summary>
    /// True while the teleport confirmation is up, and for the rest of the frame it closed on, so
    /// the A/B press that answered it is not also read by the map.
    /// </summary>
    private static bool Confirming => UnifiedPopup.IsAvailable() && UnifiedPopup.WasVisibleThisFrame();

    /// <summary>Teleporting costs coins, so it is never a single press: ask first, then pay and go.</summary>
    private static void Teleport(PortEntry? entry)
    {
        if (entry == null || Player.m_localPlayer == null || Minimap.instance == null || Confirming) return;
        Player player = Player.m_localPlayer;
        if (!player.IsTeleportable(allowAllItems: false))
        {
            player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
            return;
        }
        if (!CanAfford(player, entry))
        {
            player.Message(MessageHud.MessageType.Center, "$msg_missingrequirement");
            return;
        }
        if (!UnifiedPopup.IsAvailable())
        {
            ExecuteTeleport(entry);
            return;
        }
        string header = ControllerHints.Localize(LocalKeys.ConfirmTeleport);
        string body = Localization.instance.Localize(LocalKeys.ConfirmTeleportBody, entry.Info.portID.Name)
                      + "\n\n" + ControllerHints.Localize(CostText(player, entry));
        UnifiedPopup.Push(new YesNoPopup(header, body,
            () =>
            {
                UnifiedPopup.Pop();
                ExecuteTeleport(entry);
            },
            UnifiedPopup.Pop,
            localizeText: false,
            coverBackground: false));
    }

    private static void ExecuteTeleport(PortEntry entry)
    {
        if (Player.m_localPlayer == null || Minimap.instance == null || !s_open) return;
        Player player = Player.m_localPlayer;
        // Re-checked here: the popup may have been answered after the player lost coins or the map.
        if (!player.IsTeleportable(allowAllItems: false))
        {
            player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
            return;
        }
        if (!CanAfford(player, entry))
        {
            player.Message(MessageHud.MessageType.Center, "$msg_missingrequirement");
            return;
        }
        if (!player.NoCostCheat()) PortUI.ConsumeTeleportRequirements(player, PortUI.GetTeleportRequirements(entry.Info));
        Vector3 destination = entry.Info.position;
        Close(false); // also consumes the pad press that answered the confirmation
        Minimap.instance.SetMapMode(Minimap.MapMode.Small);
        player.TeleportTo(destination, Quaternion.identity, true);
    }

    private static bool CanAfford(Player player, PortEntry entry) =>
        player.NoCostCheat() || PortUI.HasTeleportRequirements(player, PortUI.GetTeleportRequirements(entry.Info));

    /// <summary>"Cost: 42 Coins" (red when unaffordable) or "Cost: Free". Not yet localized.</summary>
    private static string CostText(Player player, PortEntry entry)
    {
        List<Manifest.Requirement> cost = PortUI.GetTeleportRequirements(entry.Info);
        if (cost.Count == 0 || player.NoCostCheat())
        {
            return $"{LocalKeys.Cost}: <color=yellow>{LocalKeys.Free}</color>";
        }
        string color = PortUI.HasTeleportRequirements(player, cost) ? "yellow" : "red";
        return $"{LocalKeys.Cost}: " + string.Join(", ", cost.Select(requirement => $"<color={color}>{requirement.Amount} {requirement.Item.m_shared.m_name}</color>"));
    }

    private static void ToggleFavorite(PortEntry? entry)
    {
        if (entry == null || Player.m_localPlayer == null) return;
        Player.m_localPlayer.ToggleFavoritePort(entry.Info.portID);
        PortEntry? highlighted = CurrentTarget();
        RefreshFavorites();
        // Keep the gamepad highlight on the same port if it is still listed.
        s_favoriteIndex = highlighted != null ? s_favorites.IndexOf(highlighted) : -1;
        s_sidebar?.RefreshFavorites();
    }

    // Gamepads have no pointer on the map, so the sidebar cannot be aimed at. Walking it moves the
    // map so the entry lands under the crosshair, and the regular teleport path does the rest.
    private static void MoveFavoriteHighlight(int direction)
    {
        if (s_favorites.Count == 0) return;
        s_cycleIndex = -1;
        if (s_favoriteIndex < 0) s_favoriteIndex = direction > 0 ? 0 : s_favorites.Count - 1;
        else s_favoriteIndex = (s_favoriteIndex + direction + s_favorites.Count) % s_favorites.Count;
        CenterOn(s_favorites[s_favoriteIndex]);
        s_sidebar?.EnsureFavoriteVisible(s_favoriteIndex);
    }

    private static void CyclePorts(int direction)
    {
        if (s_entries.Count == 0) return;
        s_favoriteIndex = -1;
        if (s_cycleIndex < 0) s_cycleIndex = direction > 0 ? 0 : s_entries.Count - 1;
        else s_cycleIndex = (s_cycleIndex + direction + s_entries.Count) % s_entries.Count;
        CenterOn(s_entries[s_cycleIndex]);
    }

    private const float PanDuration = 0.35f;
    private static PortEntry? s_panTarget;
    private static Vector3 s_panFrom;
    private static Vector3 s_panWritten;
    private static float s_panTime;

    // Starts a short pan of the map to the entry; StepPan moves it there over PanDuration.
    private static void CenterOn(PortEntry entry)
    {
        if (Minimap.instance == null || Player.m_localPlayer == null) return;
        s_panTarget = entry;
        s_panFrom = Minimap.instance.m_mapOffset;
        s_panWritten = s_panFrom;
        s_panTime = 0f;
    }

    // Eases the map offset toward the pan target. Runs before vanilla's UpdateMap, which then
    // centers the map and refreshes the pins from the new offset like it would for a drag. Dragging
    // or the left stick changes the offset behind our back; that hands the map back to the player.
    private static void StepPan()
    {
        if (s_panTarget == null) return;
        Minimap map = Minimap.instance;
        if (map.m_mapOffset != s_panWritten)
        {
            s_panTarget = null;
            return;
        }
        s_panTime += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(s_panTime / PanDuration));
        Vector3 offset = s_panTarget.Info.position - Player.m_localPlayer.transform.position;
        map.m_mapOffset = Vector3.Lerp(s_panFrom, new Vector3(offset.x, 0f, offset.z), t);
        s_panWritten = map.m_mapOffset;
        map.m_pinUpdateRequired = true;
        if (t >= 1f) s_panTarget = null;
    }

    private static void ClearHighlight()
    {
        s_favoriteIndex = -1;
        s_cycleIndex = -1;
    }

    // ------------------------------------------------------------------ per frame

    private static Vector3 CursorWorldPoint()
    {
        Vector3 screenPoint = ZInput.IsExclusiveGamepadActive()
            ? new Vector3(Screen.width / 2f, Screen.height / 2f)
            : ZInput.pointerPosition;
        return Minimap.instance.ScreenToWorldPoint(screenPoint);
    }

    private static PortEntry? EntryNear(Vector3 worldPoint)
    {
        float radius = Minimap.instance.PinInteractRadius;
        PortEntry? best = null;
        float bestDistance = float.MaxValue;
        foreach (PortEntry entry in s_entries)
        {
            float distance = global::Utils.DistanceXZ(worldPoint, entry.Info.position);
            if (distance < radius && distance < bestDistance)
            {
                best = entry;
                bestDistance = distance;
            }
        }
        return best;
    }

    // Mirrors the takeInput check Minimap.Update makes before it reads any map input.
    private static bool TakeInput() =>
        (Chat.instance == null || !Chat.instance.HasFocus())
        && !global::Console.IsVisible()
        && !TextInput.IsVisible()
        && !Menu.IsActive()
        && !InventoryGui.IsVisible()
        && (Hud.instance == null || Hud.instance.m_buildUi == null || !Hud.instance.m_buildUi.SearchFieldFocused)
        && !ZInput.VirtualKeyboardOpen
        && !Minimap.InTextInput();

    // Runs before Minimap.Update reads the pad. A press handled here is consumed, so the vanilla
    // map code later in the same Update does not also act on it (LB/RB would otherwise cross off
    // or remove whatever pin sits under the crosshair). The d-pad is left alone: it selects and
    // filters pin icons in vanilla and that still works while picking.
    private static void GamepadInput()
    {
        // inputDelay is the grace period ShowPointOnMap starts, so the press that opened the map
        // cannot also act on it.
        if (Confirming || !ZInput.IsGamepadActive() || !TakeInput() || Minimap.instance.inputDelay > 0f) return;
        PortEntry? target = CurrentTarget();
        if (ZInput.GetButtonDown("JoyButtonA"))
        {
            ControllerHints.Consume("JoyButtonA");
            Teleport(target);
        }
        else if (ZInput.GetButtonDown("JoyButtonY"))
        {
            ControllerHints.Consume("JoyButtonY");
            ToggleFavorite(target);
        }
        else if (ZInput.GetButtonDown("JoyRStickDown"))
        {
            ControllerHints.Consume("JoyRStickDown");
            MoveFavoriteHighlight(1);
        }
        else if (ZInput.GetButtonDown("JoyRStickUp"))
        {
            ControllerHints.Consume("JoyRStickUp");
            MoveFavoriteHighlight(-1);
        }
        else if (ZInput.GetButtonDown("JoyTabRight"))
        {
            ControllerHints.Consume("JoyTabRight");
            CyclePorts(1);
        }
        else if (ZInput.GetButtonDown("JoyTabLeft"))
        {
            ControllerHints.Consume("JoyTabLeft");
            CyclePorts(-1);
        }
        else if (Mathf.Abs(ZInput.GetJoyLeftStickX()) > 0.1f || Mathf.Abs(ZInput.GetJoyLeftStickY()) > 0.1f) ClearHighlight();
    }

    private static void FrameUpdate()
    {
        if (Confirming)
        {
            // Freeze hover and ignore map input while the confirmation owns the pad and the pointer.
            if (s_open) s_sidebar?.Update(Time.deltaTime, CurrentTarget());
            MapHints.Tick();
            return;
        }
        s_hovered = EntryNear(CursorWorldPoint());
        PortEntry? target = CurrentTarget();
        foreach (PortEntry entry in s_entries)
        {
            Color color = entry == target ? HighlightColor : Color.white;
            if (entry.Pin.m_iconElement != null && entry.Pin.m_iconElement.color != color) entry.Pin.m_iconElement.color = color;
            TMP_Text? label = entry.Pin.m_NamePinData?.PinNameText;
            if (label != null && label.color != color) label.color = color;
        }

        if (!s_open) return;
        PortEntry? current = CurrentTarget();
        s_sidebar?.Update(Time.deltaTime, current);
        MapHints.SetFavorite(current?.Favorite ?? false);
        MapHints.Tick();
    }

    // ------------------------------------------------------------------ patches

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
    private static class RegisterPortPinType
    {
        [UsedImplicitly]
        private static void Postfix(Minimap __instance)
        {
            s_pinType = __instance.m_visibleIconTypes.Length;
            bool[] visible = new bool[s_pinType + 1];
            Array.Copy(__instance.m_visibleIconTypes, visible, s_pinType);
            visible[s_pinType] = true;
            __instance.m_visibleIconTypes = visible;
            Sprite? icon = MinimapTraderIcons.achorSprite != null ? MinimapTraderIcons.achorSprite : __instance.GetSprite(Minimap.PinType.Icon4);
            __instance.m_icons.Add(new Minimap.SpriteData
            {
                m_name = (Minimap.PinType)s_pinType,
                m_icon = icon,
            });
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
    private static class CloseWithMap
    {
        // Minimap.Update closes the large map on B / Escape; while the teleport confirmation is up
        // those keys belong to the popup. ExecuteTeleport closes the picker first, so its own
        // SetMapMode(Small) is not caught here.
        [UsedImplicitly]
        private static bool Prefix(Minimap.MapMode mode) => !(s_open && mode == Minimap.MapMode.Small && Confirming);

        [UsedImplicitly]
        private static void Postfix(Minimap.MapMode mode)
        {
            if (mode == Minimap.MapMode.Large || !s_open) return;
            // Small means the player closed the map by hand; anything else (death, no map) just drops the picker.
            Close(returnToPortUi: mode == Minimap.MapMode.Small);
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Update))]
    private static class UpdatePortMap
    {
        [UsedImplicitly]
        private static void Prefix(Minimap __instance)
        {
            if (!s_open || __instance.m_mode != Minimap.MapMode.Large || Player.m_localPlayer == null) return;
            GamepadInput();
            StepPan();
        }

        [UsedImplicitly]
        private static void Postfix(Minimap __instance)
        {
            if (!s_open || __instance.m_mode != Minimap.MapMode.Large || Player.m_localPlayer == null) return;
            FrameUpdate();
        }
    }

    // Port labels are always wanted, not only when zoomed in past the vanilla name threshold. The
    // vanilla name prefab is offset for a normal-size pin; a double-size pin needs the label moved
    // down by the extra half height or the two overlap.
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]
    private static class AlwaysLabelPortPins
    {
        [UsedImplicitly]
        private static void Postfix(Minimap __instance)
        {
            if (!s_open || __instance.m_mode != Minimap.MapMode.Large) return;
            foreach (PortEntry entry in s_entries)
            {
                Minimap.PinNameData? name = entry.Pin.m_NamePinData;
                GameObject? label = name?.PinNameGameObject;
                if (name == null || label == null || entry.Pin.m_uiElement == null) continue;
                if (!label.activeSelf) label.SetActive(true);
                if (entry.Pin.m_doubleSize && name.PinNameRectTransform != null)
                {
                    float extra = (entry.Pin.m_uiElement.rect.height - __instance.m_pinSizeLarge) * 0.5f;
                    if (extra > 0f) name.PinNameRectTransform.anchoredPosition = entry.Pin.m_uiElement.anchoredPosition + new Vector2(0f, -extra);
                }
            }
        }
    }

    private static PortEntry? EntryUnderPointer() =>
        s_open && Minimap.instance != null ? EntryNear(Minimap.instance.ScreenToWorldPoint(ZInput.pointerPosition)) : null;

    // Left click on a port teleports; elsewhere it crosses off the player's pin as in vanilla.
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
    private static class LeftClickTeleports
    {
        [UsedImplicitly]
        private static bool Prefix()
        {
            PortEntry? entry = EntryUnderPointer();
            if (entry == null) return true;
            if (!Confirming) Teleport(entry);
            return false;
        }
    }

    // Middle click on a port toggles it as a favorite; elsewhere it pings as in vanilla. Right
    // click is left to vanilla (remove pin) with the port pins protected below.
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapMiddleClick))]
    private static class MiddleClickFavorites
    {
        [UsedImplicitly]
        private static bool Prefix()
        {
            PortEntry? entry = EntryUnderPointer();
            if (entry == null) return true;
            if (!Confirming) ToggleFavorite(entry);
            return false;
        }
    }

    // Adding a pin on top of a port (double click, or A on the pad once the press is released)
    // would hide the port; adding pins anywhere else still works.
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapDblClick))]
    private static class NoPinOnPortByDoubleClick
    {
        [UsedImplicitly]
        private static bool Prefix() => EntryUnderPointer() == null;
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.ShowPinNameInput))]
    private static class NoPinOnPort
    {
        [UsedImplicitly]
        private static bool Prefix(Vector3 __0) => !s_open || EntryNear(__0) == null;
    }

    // Right click and RB remove the closest pin; a port pin must survive that.
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.RemovePin), typeof(Vector3), typeof(float))]
    private static class ProtectPortPins
    {
        [UsedImplicitly]
        private static bool Prefix(Minimap __instance, Vector3 pos, float radius, ref bool __result)
        {
            if (!s_open) return true;
            Minimap.PinData? closest = __instance.GetClosestPin(pos, radius);
            if (closest == null || !s_entries.Any(entry => entry.Pin == closest)) return true;
            __result = false;
            return false;
        }
    }

    // The large map's own buttons (public position, shared map) fire straight off the pad through
    // UIGamePad regardless of what the map is doing; they collide with the picker's buttons.
    [HarmonyPatch(typeof(UIGamePad), nameof(UIGamePad.ButtonPressed))]
    private static class BlockMapButtons
    {
        [UsedImplicitly]
        private static bool Prefix(UIGamePad __instance, ref bool __result)
        {
            if (!s_open || Minimap.instance == null || !__instance.transform.IsChildOf(Minimap.instance.m_largeRoot.transform)) return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnDestroy))]
    private static class ResetOnMapDestroy
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            s_open = false;
            s_origin = null;
            s_entries.Clear();
            s_favorites.Clear();
            s_hovered = null;
            s_favoriteIndex = -1;
            s_cycleIndex = -1;
            s_savedIconFilters = null;
            s_sidebar = null;
            MapHints.Reset();
        }
    }

    // ------------------------------------------------------------------ hints

    /// <summary>
    /// The picker's hints live in the map's own hint bars (KeyHints/keyboard_hints, gamepad_hints and
    /// gamepadmouse_hints) so they sit where the vanilla hints do, use the vanilla mouse glyphs, and
    /// follow UIInputHint's switching between input layouts. While a port is being picked the
    /// vanilla pin-editing entries are hidden and these clones stand in for them; zoom stays.
    /// </summary>
    private static class MapHints
    {
        private class Slot
        {
            public GameObject Root = null!;
            public TMP_Text Text = null!;
            public string Glyphs = "";
            public Func<bool, string> Label = null!; // target is a favorite -> label key
        }

        private static readonly List<Slot> s_slots = new();
        private static readonly List<GameObject> s_hiddenVanilla = new();
        private static Minimap? s_builtFor;
        private static bool s_shown;
        private static bool s_favorite;

        public static void Show(bool show)
        {
            if (show) Build();
            s_shown = show;
            foreach (GameObject vanilla in s_hiddenVanilla)
            {
                if (vanilla != null) vanilla.SetActive(!show);
            }
            foreach (Slot slot in s_slots)
            {
                if (slot.Root != null) slot.Root.SetActive(show);
            }
            if (show) Apply(force: true);
        }

        public static void SetFavorite(bool favorite)
        {
            if (favorite == s_favorite) return;
            s_favorite = favorite;
            Apply(force: false);
        }

        // UIInputHint re-enables its own layout-specific entries on an input change; keep ours in front.
        public static void Tick()
        {
            if (!s_shown) return;
            foreach (GameObject vanilla in s_hiddenVanilla)
            {
                if (vanilla != null && vanilla.activeSelf) vanilla.SetActive(false);
            }
        }

        public static void Reset()
        {
            if (Localization.instance != null)
            {
                foreach (Slot slot in s_slots)
                {
                    if (slot.Text != null) Localization.instance.RemoveTextFromCache(slot.Text);
                }
            }
            s_slots.Clear();
            s_hiddenVanilla.Clear();
            s_builtFor = null;
            s_shown = false;
        }

        private static void Build()
        {
            if (Minimap.instance == null || s_builtFor == Minimap.instance) return;
            Reset();
            s_builtFor = Minimap.instance;
            Transform root = Minimap.instance.m_largeRoot.transform;

            // Gamepad bars: plain texts, a glyph placeholder in a monospace span then the label.
            foreach (string group in new[] { "gamepad_hints", "gamepadmouse_hints" })
            {
                Transform? bar = root.Find($"KeyHints/{group}");
                Transform? template = bar != null ? bar.Find("Text - AddPin") : null;
                if (bar == null || template == null) continue;
                // Zoom and the d-pad icon filter keep working; A, LB and RB mean something else now.
                HideVanilla(bar, "Text - Zoom", "Text - HidePin");
                AddText(template, ControllerHints.ButtonA, _ => LocalKeys.Teleport);
                AddText(template, ControllerHints.ButtonY, favorite => favorite ? LocalKeys.RemoveFavorite : LocalKeys.AddFavorite);
                AddText(template, ControllerHints.Glyphs(ControllerHints.RightStickUp, ControllerHints.RightStickDown), _ => LocalKeys.HintFavorites);
                AddText(template, ControllerHints.Glyphs(ControllerHints.LeftBumper, ControllerHints.RightBumper), _ => LocalKeys.HintCyclePorts);
            }

            // Keyboard bar: an icon image plus a label per entry. Cross off pin carries the left
            // click glyph and ping the middle click glyph, so those two are the templates. Cross
            // off and remove pin still work off a port, so their vanilla entries stay.
            Transform? keyboard = root.Find("KeyHints/keyboard_hints");
            if (keyboard != null)
            {
                Transform? leftClick = keyboard.Find("CrossOffPin");
                Transform? middleClick = keyboard.Find("PingPanel");
                HideVanilla(keyboard, "CrossOffPin", "RemovePin");
                if (middleClick != null) AddKeyboard(middleClick, favorite => favorite ? LocalKeys.RemoveFavorite : LocalKeys.AddFavorite, first: true);
                if (leftClick != null) AddKeyboard(leftClick, _ => LocalKeys.Teleport, first: true);
            }
        }

        private static void HideVanilla(Transform bar, params string[] keep)
        {
            for (int i = 0; i < bar.childCount; ++i)
            {
                GameObject child = bar.GetChild(i).gameObject;
                if (Array.IndexOf(keep, child.name) >= 0 || child.name.StartsWith("MWL_") || !child.activeSelf) continue;
                s_hiddenVanilla.Add(child);
            }
        }

        private static void AddText(Transform template, string glyphs, Func<bool, string> label)
        {
            GameObject clone = Object.Instantiate(template.gameObject, template.parent);
            clone.name = "MWL_Hint_" + glyphs;
            TMP_Text? text = clone.GetComponent<TMP_Text>();
            if (text == null)
            {
                Object.Destroy(clone);
                return;
            }
            s_slots.Add(new Slot { Root = clone, Text = text, Glyphs = glyphs, Label = label });
        }

        private static void AddKeyboard(Transform template, Func<bool, string> label, bool first)
        {
            GameObject clone = Object.Instantiate(template.gameObject, template.parent);
            clone.name = "MWL_Hint_" + template.name;
            if (first) clone.transform.SetAsFirstSibling();
            Transform? labelTransform = clone.transform.Find("Label");
            TMP_Text? text = labelTransform != null ? labelTransform.GetComponent<TMP_Text>() : null;
            if (text == null)
            {
                Object.Destroy(clone);
                return;
            }
            s_slots.Add(new Slot { Root = clone, Text = text, Glyphs = "", Label = label });
        }

        private static void Apply(bool force)
        {
            if (Localization.instance == null) return;
            foreach (Slot slot in s_slots)
            {
                if (slot.Text == null) continue;
                string labelKey = slot.Label(s_favorite);
                string template = slot.Glyphs.Length > 0 ? ControllerHints.Format(slot.Glyphs, labelKey) : labelKey;
                Localization.instance.textMeshStrings.TryGetValue(slot.Text, out string? current);
                if (!force && current == template) continue;
                // Registered in the localization cache so an input layout change re-renders the
                // glyphs through the vanilla path, the same as the entries these replace.
                Localization.instance.textMeshStrings[slot.Text] = template;
                slot.Text.text = Localization.instance.Localize(template);
            }
        }
    }

    // ------------------------------------------------------------------ sidebar

    private class Sidebar
    {
        private const float Width = 330f;
        private const int MaxRows = 14;

        private readonly GameObject m_root;
        private readonly TextMeshProUGUI m_title;
        private readonly TextMeshProUGUI m_from;
        private readonly TextMeshProUGUI m_detail;
        private readonly TextMeshProUGUI m_favoritesHeader;
        private readonly Transform m_rows;
        private readonly TextMeshProUGUI m_empty;
        private readonly Row m_scrollUp;
        private readonly Row m_scrollDown;
        private readonly List<Row> m_favoriteRows = new();
        private int m_scroll;
        private float m_detailTimer;
        private PortEntry? m_detailEntry;

        public bool IsAlive => m_root != null;

        public Sidebar(Transform parent)
        {
            m_root = new GameObject("MWL_PortMapSidebar", typeof(RectTransform));
            m_root.transform.SetParent(parent, false);
            RectTransform rect = m_root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(24f, 0f);
            rect.sizeDelta = new Vector2(Width, 100f);

            Image background = m_root.AddComponent<Image>();
            ApplyPanelBackground(background);
            background.raycastTarget = true; // keeps clicks on the sidebar off the map underneath

            VerticalLayoutGroup layout = m_root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            ContentSizeFitter fitter = m_root.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            m_title = ControllerHints.CreateText(m_root.transform, "Title", 24f);
            m_title.color = new Color(1f, 0.65f, 0.2f);
            m_from = ControllerHints.CreateText(m_root.transform, "From", 16f, wrap: true);
            m_detail = ControllerHints.CreateText(m_root.transform, "Detail", 16f, wrap: true);
            LayoutElement detailLayout = m_detail.gameObject.AddComponent<LayoutElement>();
            detailLayout.minHeight = 72f;

            m_favoritesHeader = ControllerHints.CreateText(m_root.transform, "FavoritesHeader", 18f);
            m_favoritesHeader.color = new Color(1f, 0.65f, 0.2f);

            GameObject rows = new GameObject("Favorites", typeof(RectTransform));
            rows.transform.SetParent(m_root.transform, false);
            VerticalLayoutGroup rowLayout = rows.AddComponent<VerticalLayoutGroup>();
            rowLayout.spacing = 2f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            m_rows = rows.transform;
            m_empty = ControllerHints.CreateText(m_rows, "Empty", 15f);
            m_empty.color = new Color(0.8f, 0.8f, 0.8f);
            m_scrollUp = new Row(m_rows, "ScrollUp", "▲", _ => Scroll(-MaxRows));
            m_scrollDown = new Row(m_rows, "ScrollDown", "▼", _ => Scroll(MaxRows));
        }

        private static void ApplyPanelBackground(Image image)
        {
            Image? template = null;
            if (InventoryGui.instance != null && InventoryGui.instance.m_crafting != null)
            {
                Transform? bkg = InventoryGui.instance.m_crafting.Find("Bkg");
                if (bkg != null) template = bkg.GetComponent<Image>();
            }
            if (template != null && template.sprite != null)
            {
                image.sprite = template.sprite;
                image.material = template.material;
                image.type = template.type;
                image.color = template.color;
            }
            else
            {
                image.color = new Color(0f, 0f, 0f, 0.75f);
            }
        }

        public void Show()
        {
            m_root.SetActive(true);
            m_root.transform.SetAsLastSibling();
            m_scroll = 0;
            m_title.text = ControllerHints.Localize(LocalKeys.PortMap);
            m_from.text = ControllerHints.Localize($"{LocalKeys.From}: <color=orange>{s_origin?.m_name}</color>");
            m_favoritesHeader.text = ControllerHints.Localize(LocalKeys.Favorites);
            m_detailEntry = null;
            m_detailTimer = 1f;
            RefreshFavorites();
        }

        public void Hide()
        {
            if (m_root != null) m_root.SetActive(false);
        }

        public void RefreshFavorites()
        {
            if (m_root == null) return;
            int count = s_favorites.Count;
            if (m_scroll > Mathf.Max(0, count - MaxRows)) m_scroll = Mathf.Max(0, count - MaxRows);
            while (m_favoriteRows.Count < Mathf.Min(count, MaxRows))
            {
                int rowIndex = m_favoriteRows.Count;
                m_favoriteRows.Add(new Row(m_rows, $"Favorite{rowIndex}", "", button => OnRowClicked(rowIndex, button), hovering => OnRowHovered(rowIndex, hovering)));
            }
            m_empty.gameObject.SetActive(count == 0);
            m_empty.text = ControllerHints.Localize(LocalKeys.NoFavorites);
            m_scrollUp.SetActive(m_scroll > 0);
            m_scrollUp.Transform.SetSiblingIndex(1);
            for (int i = 0; i < m_favoriteRows.Count; ++i)
            {
                int index = m_scroll + i;
                bool shown = index < count;
                m_favoriteRows[i].SetActive(shown);
                if (!shown) continue;
                PortEntry entry = s_favorites[index];
                float distance = Player.m_localPlayer != null ? entry.Info.GetDistance(Player.m_localPlayer) : 0f;
                m_favoriteRows[i].SetText($"★ {entry.Info.portID.Name}  <color=#bbbbbb>({(int)distance}m)</color>");
                m_favoriteRows[i].Transform.SetSiblingIndex(2 + i);
            }
            m_scrollDown.SetActive(m_scroll + MaxRows < count);
            m_scrollDown.Transform.SetAsLastSibling();
            UpdateRowHighlight();
        }

        private void OnRowClicked(int rowIndex, PointerEventData.InputButton button)
        {
            int index = m_scroll + rowIndex;
            if (index < 0 || index >= s_favorites.Count) return;
            if (button == PointerEventData.InputButton.Left) Teleport(s_favorites[index]);
            else ToggleFavorite(s_favorites[index]); // middle or right click
        }

        // Hovering a favorite with the mouse centers the map on it, the same as walking the list
        // with the right stick, so the player can see where it is before committing.
        private void OnRowHovered(int rowIndex, bool hovering)
        {
            int index = m_scroll + rowIndex;
            if (Confirming || index < 0 || index >= s_favorites.Count) return;
            if (hovering)
            {
                s_cycleIndex = -1;
                s_favoriteIndex = index;
                CenterOn(s_favorites[index]);
            }
            else if (s_favoriteIndex == index)
            {
                s_favoriteIndex = -1;
            }
        }

        private void Scroll(int delta)
        {
            m_scroll = Mathf.Clamp(m_scroll + delta, 0, Mathf.Max(0, s_favorites.Count - MaxRows));
            RefreshFavorites();
        }

        public void EnsureFavoriteVisible(int index)
        {
            if (index < m_scroll) m_scroll = index;
            else if (index >= m_scroll + MaxRows) m_scroll = index - MaxRows + 1;
            RefreshFavorites();
        }

        private void UpdateRowHighlight()
        {
            for (int i = 0; i < m_favoriteRows.Count; ++i)
            {
                m_favoriteRows[i].SetHighlighted(s_favoriteIndex >= 0 && m_scroll + i == s_favoriteIndex);
            }
        }

        public void Update(float dt, PortEntry? target)
        {
            if (m_root == null || !m_root.activeSelf) return;
            UpdateRowHighlight();
            m_detailTimer += dt;
            if (target != m_detailEntry || m_detailTimer >= 0.5f)
            {
                m_detailEntry = target;
                m_detailTimer = 0f;
                m_detail.text = DetailText(target);
            }
        }

        private static string DetailText(PortEntry? entry)
        {
            if (entry == null || Player.m_localPlayer == null)
            {
                return ControllerHints.Localize($"<color=#bbbbbb>{LocalKeys.AimAtPort}</color>");
            }
            Player player = Player.m_localPlayer;
            string text = $"<color=orange>{entry.Info.portID.Name}</color>{(entry.Favorite ? " ★" : "")}\n";
            text += $"{LocalKeys.Distance}: <color=yellow>{(int)entry.Info.GetDistance(player)}m</color>\n";
            text += CostText(player, entry);
            return ControllerHints.Localize(text);
        }

        private class Row
        {
            private readonly GameObject m_go;
            private readonly Image m_background;
            private readonly TextMeshProUGUI m_text;
            private static readonly Color Normal = new(1f, 1f, 1f, 0.06f);
            private static readonly Color Highlighted = new(1f, 0.85f, 0.3f, 0.35f);

            public Transform Transform => m_go.transform;

            public Row(Transform parent, string name, string text, Action<PointerEventData.InputButton> onClick, Action<bool>? onHover = null)
            {
                m_go = new GameObject(name, typeof(RectTransform));
                m_go.transform.SetParent(parent, false);
                m_background = m_go.AddComponent<Image>();
                m_background.color = Normal;
                m_background.raycastTarget = true;
                LayoutElement layout = m_go.AddComponent<LayoutElement>();
                layout.minHeight = 26f;
                Button button = m_go.AddComponent<Button>();
                button.targetGraphic = m_background;
                Navigation navigation = button.navigation;
                navigation.mode = Navigation.Mode.None;
                button.navigation = navigation;
                ColorBlock colors = button.colors;
                colors.highlightedColor = new Color(1f, 1f, 1f, 2f);
                colors.pressedColor = new Color(1f, 1f, 1f, 3f);
                button.colors = colors;
                PointerRelay relay = m_go.AddComponent<PointerRelay>();
                relay.OnClick = onClick;
                relay.OnHover = onHover;
                m_text = ControllerHints.CreateText(m_go.transform, "Text", 16f);
                RectTransform textRect = m_text.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8f, 0f);
                textRect.offsetMax = new Vector2(-8f, 0f);
                m_text.alignment = TextAlignmentOptions.MidlineLeft;
                m_text.text = text;
            }

            public void SetText(string text) => m_text.text = ControllerHints.Localize(text);
            public void SetActive(bool active) => m_go.SetActive(active);
            public void SetHighlighted(bool highlighted) => m_background.color = highlighted ? Highlighted : Normal;
        }

        private class PointerRelay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
        {
            public Action<PointerEventData.InputButton>? OnClick;
            public Action<bool>? OnHover;
            public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(eventData.button);
            public void OnPointerEnter(PointerEventData eventData) => OnHover?.Invoke(true);
            public void OnPointerExit(PointerEventData eventData) => OnHover?.Invoke(false);
        }
    }
}
