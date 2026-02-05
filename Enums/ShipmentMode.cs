namespace WorkOrderApplication.API.Enums;

/// <summary>
/// กำหนดโหมดการทำงานของ ShipmentProcess
/// </summary>
public enum ShipmentMode
{
    /// <summary>
    /// เรียก AMR API อัตโนมัติ (มี ExternalId)
    /// </summary>
    ExternalApi = 0,
    
    /// <summary>
    /// ส่งแบบ Manual โดยไม่เรียก API (ไม่มี ExternalId)
    /// </summary>
    Manual = 1
}
