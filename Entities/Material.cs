using System.ComponentModel.DataAnnotations;

namespace WorkOrderApplication.API.Entities;

public class Material
{
    public int Id { get; set; } // Primary Key

    public string MaterialNumber { get; set; } = default!; // รหัสวัสดุ
    public string Description { get; set; } = default!; // รายละเอียดวัสดุ
    public int Quantity { get; set; } // จำนวนที่สั่งผลิตใน Work Order
    public int RequestPerHour { get; set; } // จำนวนที่ขอใช้ต่อชั่วโมง
    public string Unit { get; set; } = default!; // หน่วยนับ
    // -----------------------------------------------------------------------------------------------------------
    public int WorkOrderId { get; set; } // Foreign Key
    public WorkOrder WorkOrder { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public ICollection<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
    public ICollection<PreparingMaterial> PreparingMaterials { get; set; } = new List<PreparingMaterial>();
    public ICollection<ReceivedMaterial> ReceivedMaterials { get; set; } = new List<ReceivedMaterial>();
}


