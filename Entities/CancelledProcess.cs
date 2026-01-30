using System;

namespace WorkOrderApplication.API.Entities;

public class CancelledProcess
{
    public int Id { get; set; }
    public DateTime CancelledDate { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = default!; // เช่น "Out of Stock", "Customer Request"
    // -----------------------------------------------------------------------------------------------------------
    public int? CancelledByUserId { get; set; } // Nullable in case the user is deleted
    public User? CancelledBy { get; set; } // Navigation property to the Operator who cancelled
    // -----------------------------------------------------------------------------------------------------------
    public int OrderProcessId { get; set; }
    public OrderProcess OrderProcess { get; set; } = default!;
}
