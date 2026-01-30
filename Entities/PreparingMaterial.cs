

namespace WorkOrderApplication.API.Entities;

public class PreparingMaterial
{
    public int Id { get; set; } // Primary Key
    // -----------------------------------------------------------------------------------------------------------
    public int PreparingProcessId { get; set; } // Foreign Key
    public PreparingProcess PreparingProcess { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public int MaterialId { get; set; } // Foreign Key
    public Material Material { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public int PreparedQty { get; set; } // จำนวนที่จัดเตรียมจริง
}
