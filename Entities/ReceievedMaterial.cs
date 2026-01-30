namespace WorkOrderApplication.API.Entities;

public class ReceivedMaterial
{
    public int Id { get; set; } // Primary Key
    // -----------------------------------------------------------------------------------------------------------
    public int ReceivedProcessId { get; set; }
    public ReceivedProcess ReceivedProcess { get; set; } = default!;
    // -----------------------------------------------------------------------------------------------------------
    public int MaterialId { get; set; }
    public Material Material { get; set; } = default!;
    // -----------------------------------------------------------------------------------------------------------
    public int ReceivedQty { get; set; }
}