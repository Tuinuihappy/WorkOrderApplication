using System;

namespace WorkOrderApplication.API.Entities;

public class ReturnProcess
{
    public int Id { get; set; } // Primary Key
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow; // กำหนดค่าเริ่มต้นเป็นวันที่ปัจจุบัน
    public string Reason { get; set; } = default!; // เช่น "Defective Item", "Wrong Product", "Customer Changed Mind"
    // -----------------------------------------------------------------------------------------------------------
    public int ReturnByUserId { get; set; } // Foreign Key
    public User ReturnByUser { get; set; } = default!; // Navigation Property
    // -----------------------------------------------------------------------------------------------------------
    public int OrderProcessId { get; set; } // Foreign Key
    public OrderProcess OrderProcess { get; set; } = default!; // Navigation Property
}

