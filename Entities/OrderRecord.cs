

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkOrderApplication.API.Entities;

public class OrderRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string? OrderName { get; set; }
    public string LastStatus { get; set; } = "Pending";
    public int ExecutingIndex { get; set; }
    public double Progress { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string? RawResponse { get; set; }
    public string? Source { get; set; }
    public string? UpdatedBy { get; set; }
}

