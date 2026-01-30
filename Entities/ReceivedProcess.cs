namespace WorkOrderApplication.API.Entities;

public class ReceivedProcess
{
    public int Id { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    // ผู้รับของ
    public int ReceivedByUserId { get; set; }
    public User ReceivedBy { get; set; } = default!;
    public string? ShortageReason { get; set; }
    // OrderProcess
    public int OrderProcessId { get; set; }
    public OrderProcess OrderProcess { get; set; } = default!;

    // ✅ Materials ที่ถูก receive
    public ICollection<ReceivedMaterial> ReceivedMaterials { get; set; } = new List<ReceivedMaterial>();
}
