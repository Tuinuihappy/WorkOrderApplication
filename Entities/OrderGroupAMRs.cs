
namespace WorkOrderApplication.API.Entities;

public class OrderGroupAMR
{
    public int Id { get; set; }
    public int SourceStationId { get; set; }
    public string SourceStation { get; set; } = string.Empty;
    public int DestinationStationId { get; set; }
    public string DestinationStation { get; set; } = string.Empty;
    public int OrderGroupId { get; set; }
}
