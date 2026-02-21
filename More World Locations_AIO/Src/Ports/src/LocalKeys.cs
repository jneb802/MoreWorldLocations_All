using More_World_Locations_AIO.Managers;

namespace More_World_Locations_AIO;

public static class LocalKeys
{
    // organization to keep the localize keys within their own class
    public static readonly string Shipments = new Key("$label_shipment", "Shipments").GetKey();
    public static readonly string Port = new Key("$label_port", "Port").GetKey();
    public static readonly string Deliveries = new Key("$label_delivery", "Deliveries").GetKey();
    public static readonly string Manifest = new Key("$label_manifest", "Manifest").GetKey();
    public static readonly string OpenMap = new Key("$label_open_map", "Open Map").GetKey();
    public static readonly string Teleport = new Key("$label_teleport", "Teleport").GetKey();
    public static readonly string InTransit = new Key("$label_in_transit", "In Transit").GetKey();
    public static readonly string Discovered = new Key("$label_delivered", "Delivered").GetKey();
    public static readonly string Expired =  new Key("$label_expired", "Expired").GetKey();
    public static readonly string Open = new  Key("$label_open", "Open").GetKey();
    public static readonly string SuccessfullySent = new  Key("$msg_successfully_sent", "Successfully sent shipment!").GetKey();
    public static readonly string FailedToSend = new  Key("$msg_failed_to_send", "Manifest is invalid").GetKey();
    public static readonly string FailedToLoadDelivery = new Key("$msg_failed_to_load_delivery", "Containers are not empty!").GetKey();
    public static readonly string CurrentShipments = new Key("$label_current_shipments", "Current shipments").GetKey();
    public static readonly string Cost = new Key("$label_cost", "Cost").GetKey();
    public static readonly string NrOfItems = new Key("$label_nr_of_items", "Number of items").GetKey();
    public static readonly string TotalWeight = new Key("$label_total_weight", "Total weight").GetKey();
    public static readonly string Contents = new  Key("$label_contents", "Contents").GetKey();
    public static readonly string EstimatedShipTime = new Key("$label_estimated_ship_time", "Estimated shipment time").GetKey();
    public static readonly string Origin = new  Key("$label_origin", "Origin").GetKey();
    public static readonly string Destination = new  Key("$label_destination", "Destination").GetKey();
    public static readonly string PortTooltip = new Key("$tooltip_port", "List of available ports").GetKey();
    public static readonly string ShipmentTooltip = new Key("$tooltip_shipment", "List of active shipments").GetKey();
    public static readonly string DeliveryTooltip = new  Key("$tooltip_delivery", "List of active deliveries").GetKey();
    public static readonly string ManifestTooltip = new Key("$tooltip_manifest", "Purchase manifests").GetKey();
    public static readonly string TeleportTooltip = new Key("$tooltip_teleport", "Teleport to ports").GetKey();
    public static readonly string Exit = new Key("$label_exit", "Exit").GetKey();
    public static readonly string Purchase =  new Key("$label_purchase", "Purchase").GetKey();
    public static readonly string OpenDelivery = new Key("$label_open_delivery", "Open Delivery").GetKey();
    public static readonly string MaximumShipments = new Key("$msg_max_shipments", "Maximum shipments reached").GetKey();
    public static readonly string SendShipment = new Key("$msg_send_shipment", "Send shipment!").GetKey();
    public static readonly string OriginPort = new Key("$label_origin_port", "Origin port").GetKey();
    public static readonly string DestinationPort = new Key("$label_destination_port", "Destination port").GetKey();
    public static readonly string State = new  Key("$label_state", "State").GetKey();
    public static readonly string Items = new Key("$label_items", "Items").GetKey();
    public static readonly string RequiredToDefeat = new Key("$label_required_to_defeat", "Required to defeat").GetKey();
    public static readonly string Capacity = new  Key("$label_capacity", "Capacity").GetKey();
    public static readonly string CostToShip = new Key("$label_cost_to_ship", "Cost to Ship").GetKey();
    public static readonly string DeliveryCollected = new Key("$msg_delivery_collected", "Delivery marked as collected").GetKey();
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public static string ToKey(this ShipmentState state) => state switch
    {
        ShipmentState.InTransit => InTransit,
        ShipmentState.Delivered => Discovered,
        ShipmentState.Expired => Expired,
        _ => Discovered,
    };
    public class Key
    {
        private readonly LocalizeKey key;

        public string GetKey() => "$" + key.Key;

        public Key(string key, string english)
        {
            this.key = new LocalizeKey(key).English(english);
        }
    }
}