
namespace WorkOrderApplication.API.Entities;

public class OrderMaterial
{
    public int Id { get; set; } // Primary Key
    // -----------------------------------------------------------------------------------------------------------
    public int OrderProcessId { get; set; }   // Foreign Key
    public OrderProcess OrderProcess { get; set; } = default!;
    // -----------------------------------------------------------------------------------------------------------
    public int MaterialId { get; set; } // Foreign Key
    public Material Material { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public int OrderQty { get; set; } // จำนวนที่จัดเตรียมจริง
}
