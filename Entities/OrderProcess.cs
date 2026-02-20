using System.ComponentModel.DataAnnotations;
namespace WorkOrderApplication.API.Entities;

public class OrderProcess
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? TimeToUse { get; set; }
    
    public ICollection<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
    public string Status { get; set; } = "Order Placed"; // Pending, Preparing, Shipped, Received, Delivered
    public string DestinationStation { get; set; } = default!; // ✅ Required
    // -----------------------------------------------------------------------------------------------------------
    public int WorkOrderId { get; set; } // Foreign Key
    public WorkOrder WorkOrder { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public int CreatedByUserId { get; set; } // Foreign Key
    public User CreatedBy { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------   
    // Relation to Process
    public ConfirmProcess? ConfirmProcess { get; set; } // Navigation Property
    public PreparingProcess? PreparingProcess { get; set; } // Navigation Property
    public ShipmentProcess? ShipmentProcess { get; set; } // Navigation Property
    public ReceivedProcess? ReceiveProcess { get; set; } // Navigation Property
    public CancelledProcess? CancelledProcess { get; set; } // Navigation Property
    public ReturnProcess? ReturnProcess { get; set; } // Navigation Property
}