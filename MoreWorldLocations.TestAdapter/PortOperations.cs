// Extracted from valheimCLI CustomCommands.cs at 9975e28 (MIT; see THIRD-PARTY-NOTICES.md).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MoreWorldLocations.TestAdapter
{
    internal static class PortOperations
    {
        private const BindingFlags MwlReflectionFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private sealed class MwlPortContext
        {
            public Type PortType = null!;
            public Type PortInfoType = null!;
            public Type PortUiType = null!;
            public Type ManifestType = null!;
            public Type ShipmentType = null!;
            public Type ShipmentItemType = null!;
            public Type ShipmentStateType = null!;
            public Type ShipmentManagerType = null!;
            public Type PortIdType = null!;
            public FieldInfo PortViewField = null!;
            public FieldInfo PortIdField = null!;
            public FieldInfo PortNameField = null!;
            public FieldInfo PortContainersField = null!;
            public FieldInfo ContainerPlacementsField = null!;
            public PropertyInfo TempContainerIsSpawnedProperty = null!;
            public FieldInfo? PortHasOpenDeliveryField;
            public MethodInfo SpawnContainerMethod = null!;
            public MethodInfo LoadDeliveryMethod = null!;
            public MethodInfo DestroyContainersMethod = null!;
            public FieldInfo ShipmentsField = null!;
            public MethodInfo GetPortsMethod = null!;
            public FieldInfo ManifestManifestsField = null!;
            public FieldInfo ManifestNameField = null!;
            public FieldInfo ManifestCostField = null!;
            public FieldInfo ManifestChestIdField = null!;
            public FieldInfo PortUiInstanceField = null!;
            public FieldInfo PortUiSelectedDestinationField = null!;
            public FieldInfo PortUiCurrentTabField = null!;
            public MethodInfo PortUiShowMethod = null!;
            public MethodInfo PortUiOnMainButtonMethod = null!;
            public MethodInfo ShipmentSendToServerMethod = null!;
            public MethodInfo? ShipmentCanAccessMethod;
            public PropertyInfo CurrencyItemProperty = null!;
            public FieldInfo CurrencyConfigField = null!;
            public ConstructorInfo PortInfoConstructor = null!;
            public ConstructorInfo ShipmentConstructor = null!;
            public ConstructorInfo ShipmentItemConstructor = null!;
        }

        public static void PrintMwlPortStatus(float radius, Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            UnityEngine.Object[] loadedPorts = UnityEngine.Object.FindObjectsByType(context.PortType, FindObjectsSortMode.None);
            List<ZDO> portZdos = GetMwlPortZdos(context);
            int serverPortCount = portZdos.Count;
            int shipmentCount = GetMwlShipmentCount(context);
            int manifestCount = GetMwlManifestCount(context);
            object? portUi = context.PortUiInstanceField.GetValue(null);
            Player player = Player.m_localPlayer;

            string nearestText = "nearest=none";
            if (player != null && TryFindNearestMwlPort(context, radius, out object? nearestPort, out string nearestName, out float nearestDistance) && nearestPort != null)
            {
                nearestText = $"nearest='{nearestName}' distance={nearestDistance:F1}m";
            }
            else if (player != null && portZdos.Count > 0)
            {
                ZDO nearestZdo = portZdos
                    .OrderBy(portZdo => Vector3.Distance(player.transform.position, portZdo.GetPosition()))
                    .First();
                Vector3 zdoPosition = nearestZdo.GetPosition();
                nearestText = $"nearestZdo=({zdoPosition.x:F0},{zdoPosition.y:F0},{zdoPosition.z:F0}) distance={Vector3.Distance(player.transform.position, zdoPosition):F1}m";
            }

            string playerText = player != null
                ? $"player=({player.transform.position.x:F0},{player.transform.position.y:F0},{player.transform.position.z:F0})"
                : "player=none";

            addOutput($"OK: MWL_PORT_STATUS loadedPorts={loadedPorts.Length} serverPorts={serverPortCount} shipments={shipmentCount} manifests={manifestCount} portUI={(portUi != null)} {playerText} {nearestText}");
        }

        public static void GotoMwlPort(int index, Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                addOutput("ERROR: No local player found");
                return;
            }

            List<ZDO> portZdos = GetMwlPortZdos(context);
            if (portZdos.Count == 0)
            {
                addOutput("ERROR: ShipmentManager.GetPorts returned no MWL port ZDOs");
                return;
            }

            List<ZDO> orderedPorts = portZdos
                .OrderBy(portZdo => Vector3.Distance(player.transform.position, portZdo.GetPosition()))
                .ToList();
            if (index < 0 || index >= orderedPorts.Count)
            {
                addOutput($"ERROR: MWL port index {index} is out of range; totalPorts={orderedPorts.Count}");
                return;
            }

            ZDO destination = orderedPorts[index];
            Vector3 position = destination.GetPosition();
            player.TeleportTo(position + Vector3.up, player.transform.rotation, distantTeleport: true);
            addOutput($"OK: Teleported to MWL port index={index} totalPorts={orderedPorts.Count} pos=({position.x:F0},{position.y:F0},{position.z:F0})");
        }

        public static void ClearMwlShipments(Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            IDictionary? shipments = context.ShipmentsField.GetValue(null) as IDictionary;
            if (shipments == null)
            {
                addOutput("ERROR: MWL shipment dictionary is unavailable");
                return;
            }

            MethodInfo? onCollectedMethod = context.ShipmentType.GetMethod("OnCollected", MwlReflectionFlags);
            if (onCollectedMethod == null)
            {
                addOutput("ERROR: MWL Shipment.OnCollected method was not found");
                return;
            }

            List<object> snapshot = new List<object>();
            foreach (DictionaryEntry entry in shipments)
            {
                if (entry.Value != null)
                {
                    snapshot.Add(entry.Value);
                }
            }

            foreach (object shipment in snapshot)
            {
                onCollectedMethod.Invoke(shipment, null);
            }

            addOutput($"OK: MWL_CLEAR_SHIPMENTS requested={snapshot.Count}");
        }

        public static void RunMwlPortPaymentRegression(string itemPrefab, int itemCount, Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                addOutput("ERROR: No local player found");
                return;
            }

            if (!TryFindNearestMwlPort(context, 160f, out object? port, out string portName, out float portDistance) || port == null)
            {
                addOutput("ERROR: No loaded MWL port found within 160m. Teleport to an MWL port and run this again.");
                return;
            }

            ZDO? currentPortZdo = GetMwlPortZdo(context, port);
            if (currentPortZdo == null)
            {
                addOutput("ERROR: Nearest MWL port has no valid ZDO");
                return;
            }

            ZDO? destinationZdo = FindMwlDestinationZdo(context, currentPortZdo);
            if (destinationZdo == null)
            {
                addOutput("ERROR: Need at least two MWL ports available from ShipmentManager.GetPorts()");
                return;
            }

            if (!TryGetCheapestMwlManifest(context, out object? manifest, out string manifestName, out int manifestCost, out _) || manifest == null)
            {
                addOutput("ERROR: No MWL manifests are registered");
                return;
            }

            if (!TryGetMwlCurrency(context, out string currencyPrefabName, out string currencySharedName, out string currencyError))
            {
                addOutput("ERROR: " + currencyError);
                return;
            }

            if (!TryRequireNoPortContainers(context, port, "MWL payment regression", addOutput))
            {
                return;
            }

            Container? container = null;
            try
            {
                container = context.SpawnContainerMethod.Invoke(port, new object[] { manifest }) as Container;
                if (container == null)
                {
                    addOutput("ERROR: Failed to spawn MWL manifest container at nearest port");
                    return;
                }

                ItemDrop.ItemData? testItem = container.GetInventory().AddItem(itemPrefab, itemCount, 1, 0, 0L, "", cheated: true);
                if (testItem == null)
                {
                    addOutput($"ERROR: Failed to add test item prefab '{itemPrefab}' to manifest container");
                    context.DestroyContainersMethod.Invoke(port, null);
                    return;
                }

                object? containers = context.PortContainersField.GetValue(port);
                MethodInfo? getCostMethod = containers?.GetType().GetMethod("GetCost", MwlReflectionFlags);
                int expectedCost = getCostMethod != null ? Convert.ToInt32(getCostMethod.Invoke(containers, null)) : manifestCost;
                if (expectedCost <= 0)
                {
                    addOutput($"ERROR: Manifest '{manifestName}' produced non-positive shipping cost {expectedCost}");
                    context.DestroyContainersMethod.Invoke(port, null);
                    return;
                }

                Inventory inventory = player.GetInventory();
                int beforeGrantCurrency = inventory.CountItems(currencySharedName);
                inventory.AddItem(currencyPrefabName, expectedCost + 100, 1, 0, 0L, "", cheated: true);
                int beforeCurrency = inventory.CountItems(currencySharedName);
                int beforeShipments = GetMwlShipmentCount(context);

                object? destinationInfo = context.PortInfoConstructor.Invoke(new object[] { destinationZdo });
                object? portUi = context.PortUiInstanceField.GetValue(null);
                if (portUi == null)
                {
                    addOutput("ERROR: MWL PortUI.instance is null");
                    context.DestroyContainersMethod.Invoke(port, null);
                    return;
                }

                context.PortUiShowMethod.Invoke(portUi, new object[] { port });
                context.PortUiSelectedDestinationField.SetValue(portUi, destinationInfo);
                object portsTab = Enum.Parse(context.PortUiCurrentTabField.FieldType, "Ports");
                context.PortUiCurrentTabField.SetValue(portUi, portsTab);
                context.PortUiOnMainButtonMethod.Invoke(portUi, null);

                int afterCurrency = inventory.CountItems(currencySharedName);
                int afterShipments = GetMwlShipmentCount(context);
                int spent = beforeCurrency - afterCurrency;
                string result = spent == expectedCost ? "FIXED" : "BUG_PRESENT";
                addOutput($"OK: MWL_PAYMENT_REGRESSION result={result} port='{portName}' distance={portDistance:F1}m manifest='{manifestName}' item={itemPrefab}x{itemCount} expectedCost={expectedCost} currencyBeforeGrant={beforeGrantCurrency} currencyBefore={beforeCurrency} currencyAfter={afterCurrency} paymentSpent={spent} shipmentsBefore={beforeShipments} shipmentsAfter={afterShipments}");
            }
            catch (TargetInvocationException ex)
            {
                addOutput($"ERROR: MWL payment regression threw {ex.InnerException?.GetType().Name ?? ex.GetType().Name}: {ex.InnerException?.Message ?? ex.Message}");
                if (container != null)
                {
                    context.DestroyContainersMethod.Invoke(port, null);
                }
            }
            catch (Exception ex)
            {
                addOutput($"ERROR: MWL payment regression failed: {ex.GetType().Name}: {ex.Message}");
                if (container != null)
                {
                    context.DestroyContainersMethod.Invoke(port, null);
                }
            }
        }

        public static void RunMwlPortDeliveryRegression(string itemPrefab, int itemCount, Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                addOutput("ERROR: No local player found");
                return;
            }

            if (!TryFindNearestMwlPort(context, 160f, out object? port, out string portName, out float portDistance) || port == null)
            {
                addOutput("ERROR: No loaded MWL port found within 160m. Teleport to an MWL port and run this again.");
                return;
            }

            if (!TryGetCheapestMwlManifest(context, out object? manifest, out string manifestName, out _, out int chestId) || manifest == null)
            {
                addOutput("ERROR: No MWL manifests are registered");
                return;
            }

            object? shipmentsObject = context.ShipmentsField.GetValue(null);
            IDictionary? shipments = shipmentsObject as IDictionary;
            if (shipments == null)
            {
                addOutput("ERROR: MWL shipment dictionary is unavailable");
                return;
            }

            if (!TryRequireNoPortContainers(context, port, "MWL delivery regression", addOutput))
            {
                return;
            }

            string shipmentId = "cli-mwl-delivery-test-" + Guid.NewGuid().ToString("N");
            try
            {
                object originPortId = Activator.CreateInstance(context.PortIdType);
                context.PortIdType.GetField("Name")?.SetValue(originPortId, "CLI Origin");
                context.PortIdType.GetField("GUID")?.SetValue(originPortId, "cli-origin-" + Guid.NewGuid().ToString("N"));
                object destinationPortId = context.PortIdField.GetValue(port);

                object shipment = context.ShipmentConstructor.Invoke(new object[] { originPortId, destinationPortId, 1f });
                context.ShipmentType.GetField("ShipmentID")?.SetValue(shipment, shipmentId);
                context.ShipmentType.GetField("State")?.SetValue(shipment, Enum.Parse(context.ShipmentStateType, "Delivered"));
                context.ShipmentType.GetField("ArrivalTime")?.SetValue(shipment, 0d);
                context.ShipmentType.GetField("ExpirationTime")?.SetValue(shipment, ZNet.instance.GetTimeSeconds() + 3600d);

                ItemDrop.ItemData? itemData = CreateDetachedItemData(itemPrefab, itemCount);
                if (itemData == null)
                {
                    addOutput($"ERROR: Failed to create detached test item prefab '{itemPrefab}'");
                    return;
                }

                object shipmentItem = context.ShipmentItemConstructor.Invoke(new object[] { chestId, itemData });
                object? shipmentItemsObject = context.ShipmentType.GetField("Items")?.GetValue(shipment);
                IList? shipmentItems = shipmentItemsObject as IList;
                if (shipmentItems == null)
                {
                    addOutput("ERROR: MWL shipment Items list is unavailable");
                    return;
                }

                shipmentItems.Add(shipmentItem);
                shipments[shipmentId] = shipment;

                bool loaded = Convert.ToBoolean(context.LoadDeliveryMethod.Invoke(port, new object[] { shipment }));
                bool? hasOpenDelivery = PortDeliveryState.Read(port, context.PortHasOpenDeliveryField,
                    () => GetMwlPortZdo(context, port) is ZDO portZdo
                        ? PortDeliveryState.ReadSaved(fallback => portZdo.GetBool("PortHasOpenDelivery", fallback))
                        : null);
                bool fixedBehavior = loaded && hasOpenDelivery == true;
                string result = fixedBehavior ? "FIXED" : "BUG_PRESENT";

                addOutput($"OK: MWL_DELIVERY_REGRESSION result={result} port='{portName}' distance={portDistance:F1}m manifest='{manifestName}' item={itemPrefab}x{itemCount} loaded={loaded} hasOpenDelivery={(hasOpenDelivery.HasValue ? hasOpenDelivery.Value.ToString() : "unknown")} shipmentDictionaryContainsTest={shipments.Contains(shipmentId)}");
            }
            catch (TargetInvocationException ex)
            {
                addOutput($"ERROR: MWL delivery regression threw {ex.InnerException?.GetType().Name ?? ex.GetType().Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                addOutput($"ERROR: MWL delivery regression failed: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                if (shipments.Contains(shipmentId))
                {
                    shipments.Remove(shipmentId);
                }

                // The port had no chests before this probe, so every chest it holds now is the probe's own.
                context.DestroyContainersMethod.Invoke(port, null);
            }
        }

        public static void SeedMwlPortOwnershipShipment(string itemPrefab, int itemCount, Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                addOutput("ERROR: No local player found");
                return;
            }

            if (!TryFindNearestMwlPort(context, 160f, out object? port, out string portName, out float portDistance) || port == null)
            {
                addOutput("ERROR: No loaded MWL port found within 160m. Teleport to an MWL port and run this again.");
                return;
            }

            if (!TryGetCheapestMwlManifest(context, out object? manifest, out string manifestName, out _, out int chestId) || manifest == null)
            {
                addOutput("ERROR: No MWL manifests are registered");
                return;
            }

            object? shipmentsObject = context.ShipmentsField.GetValue(null);
            IDictionary? shipments = shipmentsObject as IDictionary;
            if (shipments == null)
            {
                addOutput("ERROR: MWL shipment dictionary is unavailable");
                return;
            }

            string shipmentId = "cli-mwl-ownership-test-" + Guid.NewGuid().ToString("N");
            try
            {
                object originPortId = Activator.CreateInstance(context.PortIdType);
                context.PortIdType.GetField("Name")?.SetValue(originPortId, "CLI Owner Origin");
                context.PortIdType.GetField("GUID")?.SetValue(originPortId, "cli-owner-origin-" + Guid.NewGuid().ToString("N"));
                object destinationPortId = context.PortIdField.GetValue(port);

                object shipment = context.ShipmentConstructor.Invoke(new object[] { originPortId, destinationPortId, 1f });
                context.ShipmentType.GetField("ShipmentID")?.SetValue(shipment, shipmentId);
                context.ShipmentType.GetField("State")?.SetValue(shipment, Enum.Parse(context.ShipmentStateType, "Delivered"));
                context.ShipmentType.GetField("ArrivalTime")?.SetValue(shipment, 0d);
                context.ShipmentType.GetField("ExpirationTime")?.SetValue(shipment, ZNet.instance.GetTimeSeconds() + 3600d);

                ItemDrop.ItemData? itemData = CreateDetachedItemData(itemPrefab, itemCount);
                if (itemData == null)
                {
                    addOutput($"ERROR: Failed to create detached test item prefab '{itemPrefab}'");
                    return;
                }

                object shipmentItem = context.ShipmentItemConstructor.Invoke(new object[] { chestId, itemData });
                object? shipmentItemsObject = context.ShipmentType.GetField("Items")?.GetValue(shipment);
                IList? shipmentItems = shipmentItemsObject as IList;
                if (shipmentItems == null)
                {
                    addOutput("ERROR: MWL shipment Items list is unavailable");
                    return;
                }

                int beforeShipments = shipments.Count;
                shipmentItems.Add(shipmentItem);
                context.ShipmentSendToServerMethod.Invoke(shipment, null);

                string destinationPortGuid = GetMwlPortIdGuid(context, destinationPortId);
                addOutput($"OK: MWL_OWNERSHIP_SEED shipmentId={shipmentId} owner='{player.GetPlayerName()}' playerId={player.GetPlayerID()} port='{portName}' portGuid={destinationPortGuid} distance={portDistance:F1}m manifest='{manifestName}' item={itemPrefab}x{itemCount} shipmentsBefore={beforeShipments} shipmentsLocalAfter={shipments.Count}");
            }
            catch (TargetInvocationException ex)
            {
                addOutput($"ERROR: MWL ownership seed threw {ex.InnerException?.GetType().Name ?? ex.GetType().Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                addOutput($"ERROR: MWL ownership seed failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public static void CheckMwlPortOwnershipShipment(string shipmentId, string expectation, Action<string> addOutput)
        {
            if (!TryGetMwlPortContext(addOutput, out MwlPortContext? context) || context == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                addOutput("ERROR: No local player found");
                return;
            }

            object? shipmentsObject = context.ShipmentsField.GetValue(null);
            IDictionary? shipments = shipmentsObject as IDictionary;
            if (shipments == null)
            {
                addOutput("ERROR: MWL shipment dictionary is unavailable");
                return;
            }

            if (!shipments.Contains(shipmentId))
            {
                addOutput($"ERROR: MWL_OWNERSHIP_CHECK shipmentId={shipmentId} found=false shipments={shipments.Count}");
                return;
            }

            object? shipment = shipments[shipmentId];
            if (shipment == null)
            {
                addOutput($"ERROR: MWL_OWNERSHIP_CHECK shipmentId={shipmentId} found=true shipment=null shipments={shipments.Count}");
                return;
            }

            string normalizedExpectation = expectation.Equals("allowed", StringComparison.OrdinalIgnoreCase) ? "allowed" : "blocked";
            // Without MWL's ownership gate, access is unknown rather than assumed.
            bool hasOwnershipGate = context.ShipmentCanAccessMethod != null;
            bool? canAccess = null;
            if (context.ShipmentCanAccessMethod != null)
            {
                canAccess = Convert.ToBoolean(context.ShipmentCanAccessMethod.Invoke(shipment, new object[] { player }));
            }

            string destinationPortId = Convert.ToString(context.ShipmentType.GetField("DestinationPortID")?.GetValue(shipment)) ?? "";
            bool loaded = false;
            bool destinationLoaded = false;
            string portName = "";
            float portDistance = 0f;
            object? port = FindLoadedMwlPortByGuid(context, destinationPortId, out portName, out portDistance);
            if (port != null)
            {
                if (!TryRequireNoPortContainers(context, port, "MWL ownership check", addOutput))
                {
                    return;
                }

                destinationLoaded = true;
                try
                {
                    loaded = Convert.ToBoolean(context.LoadDeliveryMethod.Invoke(port, new object[] { shipment }));
                }
                catch (TargetInvocationException ex)
                {
                    addOutput($"ERROR: MWL ownership check load threw {ex.InnerException?.GetType().Name ?? ex.GetType().Name}: {ex.InnerException?.Message ?? ex.Message}");
                    return;
                }
                finally
                {
                    // The port had no chests before this load, so every chest it holds now is the check's own.
                    context.DestroyContainersMethod.Invoke(port, null);
                }
            }
            else if (TryGetMwlPortZdoByGuid(context, destinationPortId, out ZDO? destinationZdo) && destinationZdo != null)
            {
                Vector3 position = destinationZdo.GetPosition();
                player.TeleportTo(position + Vector3.up, player.transform.rotation, distantTeleport: true);
                addOutput($"OK: MWL_OWNERSHIP_CHECK shipmentId={shipmentId} found=true destinationLoaded=false teleported=true destinationPortGuid={destinationPortId} retry=true");
                return;
            }

            string result;
            if (normalizedExpectation == "allowed")
            {
                result = canAccess == true && loaded ? "OWNER_ACCESS_OK" : "OWNER_ACCESS_FAILED";
            }
            else
            {
                result = canAccess == false && !loaded ? "FIXED" : "BUG_PRESENT";
            }

            addOutput($"OK: MWL_OWNERSHIP_CHECK result={result} expectation={normalizedExpectation} shipmentId={shipmentId} player='{player.GetPlayerName()}' playerId={player.GetPlayerID()} hasOwnershipGate={hasOwnershipGate} canAccess={(canAccess.HasValue ? canAccess.Value.ToString() : "unknown")} loaded={loaded} destinationLoaded={destinationLoaded} destinationPortGuid={destinationPortId} port='{portName}' distance={portDistance:F1}m shipments={shipments.Count}");
        }

        private static bool TryGetMwlPortContext(Action<string> addOutput, out MwlPortContext? context)
        {
            context = null;
            Type? portType = FindTypeByFullName("More_World_Locations_AIO.Port");
            Type? portUiType = FindTypeByFullName("More_World_Locations_AIO.PortUI");
            Type? manifestType = FindTypeByFullName("More_World_Locations_AIO.Manifest");
            Type? shipmentType = FindTypeByFullName("More_World_Locations_AIO.Shipment");
            Type? shipmentItemType = FindTypeByFullName("More_World_Locations_AIO.ShipmentItem");
            Type? shipmentStateType = FindTypeByFullName("More_World_Locations_AIO.ShipmentState");
            Type? shipmentManagerType = FindTypeByFullName("More_World_Locations_AIO.ShipmentManager");
            if (portType == null || portUiType == null || manifestType == null || shipmentType == null || shipmentItemType == null || shipmentStateType == null || shipmentManagerType == null)
            {
                addOutput("ERROR: More_World_Locations_AIO port runtime types are not loaded");
                return false;
            }

            Type? portInfoType = portType.GetNestedType("PortInfo", MwlReflectionFlags);
            Type? tempContainerType = portType.GetNestedType("TempContainer", MwlReflectionFlags);
            Type? portIdType = shipmentManagerType.GetNestedType("PortID", MwlReflectionFlags);
            if (portInfoType == null || tempContainerType == null || portIdType == null)
            {
                addOutput("ERROR: MWL PortInfo, Port.TempContainer or ShipmentManager.PortID type was not found");
                return false;
            }

            FieldInfo portContainersField = RequireField(portType, "m_containers");

            MwlPortContext resolved = new MwlPortContext
            {
                PortType = portType,
                PortInfoType = portInfoType,
                PortUiType = portUiType,
                ManifestType = manifestType,
                ShipmentType = shipmentType,
                ShipmentItemType = shipmentItemType,
                ShipmentStateType = shipmentStateType,
                ShipmentManagerType = shipmentManagerType,
                PortIdType = portIdType,
                PortViewField = RequireField(portType, "m_view"),
                PortIdField = RequireField(portType, "m_portID"),
                PortNameField = RequireField(portType, "m_name"),
                PortContainersField = portContainersField,
                ContainerPlacementsField = RequireField(portContainersField.FieldType, "Placements"),
                TempContainerIsSpawnedProperty = RequireProperty(tempContainerType, "IsSpawned"),
                PortHasOpenDeliveryField = portType.GetField("m_hasOpenDelivery", MwlReflectionFlags),
                SpawnContainerMethod = RequireMethod(portType, "SpawnContainer"),
                LoadDeliveryMethod = RequireMethod(portType, "LoadDelivery"),
                DestroyContainersMethod = RequireMethod(portType, "DestroyContainers"),
                ShipmentsField = RequireField(shipmentManagerType, "Shipments"),
                GetPortsMethod = RequireMethod(shipmentManagerType, "GetPorts"),
                ManifestManifestsField = RequireField(manifestType, "Manifests"),
                ManifestNameField = RequireField(manifestType, "Name"),
                ManifestCostField = RequireField(manifestType, "CostToShip"),
                ManifestChestIdField = RequireField(manifestType, "ChestStableHashCode"),
                PortUiInstanceField = RequireField(portUiType, "instance"),
                PortUiSelectedDestinationField = RequireField(portUiType, "m_selectedDestination"),
                PortUiCurrentTabField = RequireField(portUiType, "m_currentTab"),
                PortUiShowMethod = RequireMethod(portUiType, "Show"),
                PortUiOnMainButtonMethod = RequireMethod(portUiType, "OnMainButton"),
                ShipmentSendToServerMethod = RequireMethod(shipmentType, "SendToServer"),
                ShipmentCanAccessMethod = shipmentType.GetMethod("CanAccess", MwlReflectionFlags),
                CurrencyItemProperty = RequireProperty(shipmentManagerType, "CurrencyItem"),
                CurrencyConfigField = RequireField(shipmentManagerType, "CurrencyConfig"),
                PortInfoConstructor = RequireConstructor(portInfoType, new Type[] { typeof(ZDO) }),
                ShipmentConstructor = RequireConstructor(shipmentType, new Type[] { portIdType, portIdType, typeof(float) }),
                ShipmentItemConstructor = RequireConstructor(shipmentItemType, new Type[] { typeof(int), typeof(ItemDrop.ItemData) })
            };

            context = resolved;
            return true;
        }

        // Resolve only the currently registered MWL instance, never old ScriptEngine assemblies.
        private static Type? FindTypeByFullName(string fullName)
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(Plugin.MwlGuid, out var plugin) && plugin.Instance != null
                ? plugin.Instance.GetType().Assembly.GetType(fullName) : null;
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo? field = type.GetField(fieldName, MwlReflectionFlags);
            if (field == null)
            {
                throw new MissingFieldException(type.FullName, fieldName);
            }

            return field;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo? method = type.GetMethod(methodName, MwlReflectionFlags);
            if (method == null)
            {
                throw new MissingMethodException(type.FullName, methodName);
            }

            return method;
        }

        private static PropertyInfo RequireProperty(Type type, string propertyName)
        {
            PropertyInfo? property = type.GetProperty(propertyName, MwlReflectionFlags);
            if (property == null)
            {
                throw new MissingMemberException(type.FullName, propertyName);
            }

            return property;
        }

        private static ConstructorInfo RequireConstructor(Type type, Type[] parameterTypes)
        {
            ConstructorInfo? constructor = type.GetConstructor(MwlReflectionFlags, null, parameterTypes, null);
            if (constructor == null)
            {
                throw new MissingMethodException(type.FullName, ".ctor");
            }

            return constructor;
        }

        private static bool TryFindNearestMwlPort(MwlPortContext context, float radius, out object? nearestPort, out string nearestName, out float nearestDistance)
        {
            nearestPort = null;
            nearestName = "";
            nearestDistance = float.MaxValue;
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return false;
            }

            UnityEngine.Object[] ports = UnityEngine.Object.FindObjectsByType(context.PortType, FindObjectsSortMode.None);
            foreach (UnityEngine.Object candidate in ports)
            {
                Component? component = candidate as Component;
                if (component == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(player.transform.position, component.transform.position);
                if (distance > radius || distance >= nearestDistance)
                {
                    continue;
                }

                nearestPort = candidate;
                nearestName = Convert.ToString(context.PortNameField.GetValue(candidate)) ?? candidate.name;
                nearestDistance = distance;
            }

            return nearestPort != null;
        }

        // Probes clean up by destroying the port's chests. Chests (and items) that existed
        // before a probe are not the probe's to destroy, so refuse without mutating anything.
        private static bool TryRequireNoPortContainers(MwlPortContext context, object port, string probeName, Action<string> addOutput)
        {
            object? containers = context.PortContainersField.GetValue(port);
            IList? placements = containers != null ? context.ContainerPlacementsField.GetValue(containers) as IList : null;
            if (placements == null || placements.Count == 0)
            {
                addOutput($"ERROR: {probeName} refused: the port's chest placements are not initialized, so existing chests cannot be ruled out");
                return false;
            }

            int spawned = 0;
            foreach (object? placement in placements)
            {
                if (placement != null && Convert.ToBoolean(context.TempContainerIsSpawnedProperty.GetValue(placement)))
                {
                    spawned++;
                }
            }

            if (spawned > 0)
            {
                addOutput($"ERROR: {probeName} refused: the port already has {spawned} chest(s) which the probe would destroy; empty and remove them first");
                return false;
            }

            return true;
        }

        private static ZDO? GetMwlPortZdo(MwlPortContext context, object port)
        {
            ZNetView? view = context.PortViewField.GetValue(port) as ZNetView;
            if (view == null || !view.IsValid())
            {
                return null;
            }

            return view.GetZDO();
        }

        private static ZDO? FindMwlDestinationZdo(MwlPortContext context, ZDO currentPortZdo)
        {
            List<ZDO> ports = GetMwlPortZdos(context);
            foreach (ZDO zdo in ports)
            {
                if (!zdo.m_uid.Equals(currentPortZdo.m_uid))
                {
                    return zdo;
                }
            }

            return null;
        }

        private static object? FindLoadedMwlPortByGuid(MwlPortContext context, string portGuid, out string portName, out float portDistance)
        {
            portName = "";
            portDistance = 0f;
            if (string.IsNullOrEmpty(portGuid))
            {
                return null;
            }

            Player player = Player.m_localPlayer;
            UnityEngine.Object[] ports = UnityEngine.Object.FindObjectsByType(context.PortType, FindObjectsSortMode.None);
            foreach (UnityEngine.Object candidate in ports)
            {
                object? portId = context.PortIdField.GetValue(candidate);
                if (!string.Equals(GetMwlPortIdGuid(context, portId), portGuid, StringComparison.Ordinal))
                {
                    continue;
                }

                Component? component = candidate as Component;
                portName = Convert.ToString(context.PortNameField.GetValue(candidate)) ?? candidate.name;
                portDistance = player != null && component != null
                    ? Vector3.Distance(player.transform.position, component.transform.position)
                    : 0f;
                return candidate;
            }

            return null;
        }

        private static bool TryGetMwlPortZdoByGuid(MwlPortContext context, string portGuid, out ZDO? portZdo)
        {
            portZdo = null;
            foreach (ZDO zdo in GetMwlPortZdos(context))
            {
                object portId = Activator.CreateInstance(context.PortIdType);
                context.PortIdType.GetField("GUID")?.SetValue(portId, zdo.GetString("PortGUID".GetStableHashCode()));
                if (!string.Equals(GetMwlPortIdGuid(context, portId), portGuid, StringComparison.Ordinal))
                {
                    continue;
                }

                portZdo = zdo;
                return true;
            }

            return false;
        }

        private static string GetMwlPortIdGuid(MwlPortContext context, object? portId)
        {
            if (portId == null)
            {
                return "";
            }

            return Convert.ToString(context.PortIdType.GetField("GUID")?.GetValue(portId)) ?? "";
        }

        private static List<ZDO> GetMwlPortZdos(MwlPortContext context)
        {
            List<ZDO> portZdos = new List<ZDO>();
            object? portsObject = context.GetPortsMethod.Invoke(null, null);
            IEnumerable? ports = portsObject as IEnumerable;
            if (ports == null)
            {
                return portZdos;
            }

            foreach (object zdoObject in ports)
            {
                ZDO? zdo = zdoObject as ZDO;
                if (zdo != null)
                {
                    portZdos.Add(zdo);
                }
            }

            return portZdos;
        }

        private static bool TryGetCheapestMwlManifest(MwlPortContext context, out object? manifest, out string manifestName, out int manifestCost, out int chestId)
        {
            manifest = null;
            manifestName = "";
            manifestCost = int.MaxValue;
            chestId = 0;
            IDictionary? manifests = context.ManifestManifestsField.GetValue(null) as IDictionary;
            if (manifests == null || manifests.Count == 0)
            {
                return false;
            }

            foreach (DictionaryEntry entry in manifests)
            {
                object? candidate = entry.Value;
                if (candidate == null)
                {
                    continue;
                }

                int candidateCost = Convert.ToInt32(context.ManifestCostField.GetValue(candidate));
                if (candidateCost >= manifestCost)
                {
                    continue;
                }

                manifest = candidate;
                manifestName = Convert.ToString(context.ManifestNameField.GetValue(candidate)) ?? "Manifest";
                manifestCost = candidateCost;
                chestId = Convert.ToInt32(context.ManifestChestIdField.GetValue(candidate));
            }

            return manifest != null;
        }

        private static int GetMwlShipmentCount(MwlPortContext context)
        {
            ICollection? shipments = context.ShipmentsField.GetValue(null) as ICollection;
            return shipments?.Count ?? -1;
        }

        private static int GetMwlManifestCount(MwlPortContext context)
        {
            ICollection? manifests = context.ManifestManifestsField.GetValue(null) as ICollection;
            return manifests?.Count ?? -1;
        }

        // MWL charges ShipmentManager.CurrencyItem: the m_itemData of the prefab named by
        // CurrencyConfig. That prefab data has no m_dropPrefab, so resolve the configured
        // prefab and require it to be the same item MWL charges. Never assume Coins.
        private static bool TryGetMwlCurrency(MwlPortContext context, out string prefabName, out string sharedName, out string error)
        {
            prefabName = "";
            sharedName = "";
            string configuredPrefab = (context.CurrencyConfigField.GetValue(null) as BepInEx.Configuration.ConfigEntry<string>)?.Value ?? "";
            ItemDrop.ItemData? currencyItem = context.CurrencyItemProperty.GetValue(null) as ItemDrop.ItemData;
            if (currencyItem == null || currencyItem.m_shared == null || string.IsNullOrEmpty(currencyItem.m_shared.m_name))
            {
                error = $"MWL currency item is unresolved (configured prefab '{configuredPrefab}')";
                return false;
            }

            GameObject? prefab = ObjectDB.instance != null && configuredPrefab.Length > 0 ? ObjectDB.instance.GetItemPrefab(configuredPrefab) : null;
            ItemDrop? itemDrop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            if (prefab == null || itemDrop == null || !ReferenceEquals(itemDrop.m_itemData, currencyItem))
            {
                error = $"MWL currency prefab could not be resolved: configured prefab '{configuredPrefab}' is not the item MWL charges ('{currencyItem.m_shared.m_name}')";
                return false;
            }

            prefabName = prefab.name;
            sharedName = currencyItem.m_shared.m_name;
            error = "";
            return true;
        }

        private static ItemDrop.ItemData? CreateDetachedItemData(string itemPrefab, int itemCount)
        {
            if (ObjectDB.instance == null)
            {
                return null;
            }

            GameObject prefab = ObjectDB.instance.GetItemPrefab(itemPrefab);
            if (prefab == null)
            {
                return null;
            }

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                return null;
            }

            return DetachedItem.Create(itemDrop.m_itemData, prefab, itemCount);
        }

    }
}
