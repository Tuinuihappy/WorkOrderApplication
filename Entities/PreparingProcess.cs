using System;
namespace WorkOrderApplication.API.Entities;

public class PreparingProcess
{
    public int Id { get; set; } // Primary Key
    public DateTime PreparedDate { get; set; } = DateTime.UtcNow; // วันที่จัดเตรียม
    // -----------------------------------------------------------------------------------------------------------
    public int PreparingByUserId { get; set; } // Foreign Key
    public User PreparingBy { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public int OrderProcessId { get; set; } // Foreign Key
    public OrderProcess OrderProcess { get; set; } = default!; // Navigation Property
    public string? ShortageReason { get; set; }
    // -----------------------------------------------------------------------------------------------------------
    public ICollection<PreparingMaterial> PreparingMaterials { get; set; } = new List<PreparingMaterial>(); // Navigation Property
}