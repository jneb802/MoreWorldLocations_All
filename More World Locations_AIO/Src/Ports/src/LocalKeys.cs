namespace More_World_Locations_AIO;

public static class LocalKeys
{
    public static readonly string Shipments = "$label_shipment";
    public static readonly string Deliveries = "$label_delivery";
    public static readonly string Buy = "$label_port_buy";
    public static readonly string Send = "$label_port_send";
    public static readonly string Receive = "$label_port_receive";
    public static readonly string Destinations = "$label_port_destinations";
    public static readonly string Outgoing = "$label_port_outgoing";
    public static readonly string NoOutgoing = "$msg_port_no_outgoing";
    public static readonly string NoIncoming = "$msg_port_no_incoming";
    public static readonly string OpenMap = "$label_open_map";
    public static readonly string AddFavorite = "$label_add_favorite";
    public static readonly string RemoveFavorite = "$label_remove_favorite";
    public static readonly string Teleport = "$label_teleport";
    public static readonly string InTransit = "$label_in_transit";
    public static readonly string Discovered = "$label_delivered";
    public static readonly string Expired = "$label_expired";
    public static readonly string Open = "$label_open";
    public static readonly string SuccessfullySent = "$msg_successfully_sent";
    public static readonly string FailedToSend = "$msg_failed_to_send";
    public static readonly string FailedToLoadDelivery = "$msg_failed_to_load_delivery";
    public static readonly string CurrentShipments = "$label_current_shipments";
    public static readonly string Cost = "$label_cost";
    public static readonly string NrOfItems = "$label_nr_of_items";
    public static readonly string TotalWeight = "$label_total_weight";
    public static readonly string Contents = "$label_contents";
    public static readonly string EstimatedShipTime = "$label_estimated_ship_time";
    public static readonly string Origin = "$label_origin";
    public static readonly string Destination = "$label_destination";
    public static readonly string SendTooltip = "$tooltip_port_send";
    public static readonly string ReceiveTooltip = "$tooltip_port_receive";
    public static readonly string BuyTooltip = "$tooltip_port_buy";
    public static readonly string DestinationsTooltip = "$tooltip_port_destinations";
    public static readonly string OutgoingTooltip = "$tooltip_port_outgoing";
    public static readonly string TeleportTooltip = "$tooltip_teleport";
    public static readonly string Exit = "$label_exit";
    public static readonly string Purchase = "$label_purchase";
    public static readonly string OpenDelivery = "$label_open_delivery";
    public static readonly string MaximumShipments = "$msg_max_shipments";
    public static readonly string SendShipment = "$msg_send_shipment";
    public static readonly string OriginPort = "$label_origin_port";
    public static readonly string DestinationPort = "$label_destination_port";
    public static readonly string State = "$label_state";
    public static readonly string Items = "$label_items";
    public static readonly string RequiredToDefeat = "$label_required_to_defeat";
    public static readonly string Capacity = "$label_capacity";
    public static readonly string CostToShip = "$label_cost_to_ship";
    public static readonly string DeliveryCollected = "$msg_delivery_collected";
    public static readonly string ShipmentNotOwned = "This shipment belongs to another player";

    public static string ToKey(this ShipmentState state) => state switch
    {
        ShipmentState.InTransit => InTransit,
        ShipmentState.Delivered => Discovered,
        ShipmentState.Expired => Expired,
        _ => Discovered,
    };
}
