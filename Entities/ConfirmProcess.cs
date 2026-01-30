using System;

namespace WorkOrderApplication.API.Entities;

public class ConfirmProcess
{
    public int Id { get; set; }
    // -----------------------------------------------------------------------------------------------------------
    public int OrderProcessId { get; set; }
    public OrderProcess OrderProcess { get; set; } = default!;
    // -----------------------------------------------------------------------------------------------------------
    public DateTime ConfirmedDate { get; set; } = DateTime.UtcNow;
}

