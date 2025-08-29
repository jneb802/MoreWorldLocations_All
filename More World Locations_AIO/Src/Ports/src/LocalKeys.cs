using More_World_Locations_AIO.Managers;

namespace More_World_Locations_AIO;

public static class LocalKeys
{
    // organization to keep the localize keys within their own class
    public static readonly string ShipmentLabel = new Key("$label_shipment", "Shipment").GetKey();
    public static readonly string PortLabel = new Key("$label_port", "Port").GetKey();
    public static readonly string DeliveryLabel = new Key("$label_delivery", "Delivery").GetKey();
    public static readonly string ManifestLabel = new Key("$label_manifest", "Manifest").GetKey();
    public static readonly string OpenMapLabel = new Key("$label_open_map", "Open Map").GetKey();
    public static readonly string TeleportLabel = new Key("$label_teleport", "Teleport").GetKey();
    public static readonly string InTransitLabel = new Key("$label_in_transit", "In Transit").GetKey();
    public static readonly string DeliveredLabel = new Key("$label_delivered", "Delivered").GetKey();
    public static readonly string ExpiredLabel =  new Key("$label_expired", "Expired").GetKey();
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
    public static readonly string Deliveries = new Key("$label_deliveries", "Deliveries").GetKey();
    public static readonly string Origin = new  Key("$label_origin", "Origin").GetKey();
    
    
    
    
    
    public static string ToKey(this ShipmentState state) => state switch
    {
        ShipmentState.InTransit => InTransitLabel,
        ShipmentState.Delivered => DeliveredLabel,
        ShipmentState.Expired => ExpiredLabel,
        _ => DeliveredLabel,
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