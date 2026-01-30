using System;

namespace WorkOrderApplication.API.Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string EmployeeId { get; set; } = default!; // รหัสพนักงาน
    public string Position { get; set; } = default!; // เช่น Operator, Supervisor, Engineer
    public string Department { get; set; } = default!; // เช่น Production, Quality
    public string Shift { get; set; } = default!;   // Day, Night
    public string? ContactNumber { get; set; } // เบอร์โทรศัพท์
    public string Email { get; set; } = default!; // อีเมล
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
