namespace WorkOrderApplication.API.Constants;

public static class SignalRGroups
{
    public const string AllOrders = "orders-all";
    public static string OrderDetails(int id) => $"order-{id}";
}

public static class SignalREvents
{
    public const string OrderProcessCreated = "OrderProcessCreated";
    public const string OrderProcessUpdated = "OrderProcessUpdated";
    public const string OrderProcessDeleted = "OrderProcessDeleted";

    public const string ConfirmCreated = "ConfirmCreated";
    public const string ConfirmUpdated = "ConfirmUpdated";
    public const string ConfirmDeleted = "ConfirmDeleted";

    public const string PreparingCreated = "PreparingCreated";
    public const string PreparingUpdated = "PreparingUpdated";
    public const string PreparingDeleted = "PreparingDeleted";

    public const string ShipmentCreated = "ShipmentCreated";
    public const string ShipmentUpdated = "ShipmentUpdated";
    public const string ShipmentArrived = "ShipmentArrived";
    public const string ShipmentDeleted = "ShipmentDeleted";
    public const string ShipmentStateChanged = "ShipmentStateChanged";

    public const string ReceivedCreated = "ReceivedCreated";
    public const string ReceivedUpdated = "ReceivedUpdated";
    public const string ReceivedDeleted = "ReceivedDeleted";

    public const string OrderRecordUpdated = "OrderRecordUpdated";
    public const string OrderMissionsUpdated = "OrderMissionsUpdated";
}
