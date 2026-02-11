using System.ComponentModel.DataAnnotations;

namespace WorkOrderApplication.API.Entities;

public class Material
{
    public int Id { get; set; } // Primary Key

    public string MaterialNumber { get; set; } = default!; // รหัสวัสดุ
    public string Description { get; set; } = default!; // รายละเอียดวัสดุ
    public decimal Quantity { get; set; } // Reqmnt qty
    public decimal WithdrawnQuantity { get; set; } // Qty wthdrn
    public string Unit { get; set; } = default!; // BUn
    public string? OperationActivity { get; set; } // OpAc
    public string? SortString { get; set; } // SortStrng
    public string? StorageLocation { get; set; } // SLoc
    // -----------------------------------------------------------------------------------------------------------
    public int WorkOrderId { get; set; } // Foreign Key
    public WorkOrder WorkOrder { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public ICollection<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
    public ICollection<PreparingMaterial> PreparingMaterials { get; set; } = new List<PreparingMaterial>();
    public ICollection<ReceivedMaterial> ReceivedMaterials { get; set; } = new List<ReceivedMaterial>();
}


