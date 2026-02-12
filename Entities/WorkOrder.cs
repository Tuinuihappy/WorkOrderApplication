using System.ComponentModel.DataAnnotations;

namespace WorkOrderApplication.API.Entities;

/// <summary>
/// ✅ Entity หลักสำหรับ Work Order (คำสั่งการผลิต)
/// เก็บข้อมูลเกี่ยวกับการสั่งผลิต เช่น เลขที่งาน, ไลน์การผลิต, โมเดลสินค้า, จำนวนผลิต,
/// ผู้สร้าง/ผู้แก้ไข, วันเวลา และความสัมพันธ์กับ Material และ OrderProcess
/// </summary>
public class WorkOrder
{
    // -------------------- Primary Key --------------------
    public int Id { get; set; }  // Primary Key ของตาราง WorkOrders

    // -------------------- ข้อมูลพื้นฐาน --------------------
    public string Order { get; set; } = default!; // รหัส WorkOrder (Unique)
    public string OrderType { get; set; } = default!;       // Order Type
    public string Plant { get; set; } = default!;           // Plant
    public string Material { get; set; } = default!;        // Material information
    public int Quantity { get; set; }                       // จำนวนที่จะผลิต
    public string Unit { get; set; } = "PCE";               // หน่วยนับ (Default: PCE)
    public DateTime? BasicFinishDate { get; set; }          // Basic Finish Date

    // -------------------- ความสัมพันธ์กับ Material --------------------
    public ICollection<Material> Materials { get; set; } = new List<Material>();  // รายการวัสดุที่เกี่ยวข้องกับ WorkOrder

    // -------------------- วันเวลา --------------------
    public DateTime CreatedDate { get; set; }               // วันที่สร้าง WorkOrder
    public DateTime? UpdatedDate { get; set; }              // วันที่แก้ไขล่าสุด (nullable)

    // -------------------- ความสัมพันธ์กับ OrderProcess --------------------
    public ICollection<OrderProcess> OrderProcesses { get; set; } = new List<OrderProcess>(); // แต่ละ WorkOrder อาจมีหลายขั้นตอนการดำเนินการ (OrderProcess)
}
