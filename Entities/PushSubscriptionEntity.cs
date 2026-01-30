using System;

namespace WorkOrderApplication.API.Entities;

public class PushSubscriptionEntity
{
    public int Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? UserId { get; set; } // optional mapping ผู้ใช้
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
