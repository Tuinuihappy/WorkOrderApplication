    using System;
    using WorkOrderApplication.API.Enums;

    namespace WorkOrderApplication.API.Entities;

    public class ShipmentProcess
    {
        public int Id { get; set; }
        
        // 🔹 โหมดการส่ง
        public ShipmentMode ShipmentMode { get; set; } = ShipmentMode.ExternalApi;
        
        // 🔹 สถานีต้นทาง–ปลายทาง
        public int SourceStationId { get; set; }
        public string SourceStation { get; set; } = string.Empty;
        
        public int DestinationStationId { get; set; }
        public string DestinationStation { get; set; } = string.Empty;
        
        // 🔹 Mapping จาก OrderGroupAMR
        public int OrderGroupId { get; set; }

        // 🔹 ข้อมูลจาก External API
        public int ExternalId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string OrderName { get; set; } = string.Empty;
        public DateTime? ArrivalTime { get; set; }

        // 🔹 Mirror Fields (อัปเดตจาก BackgroundService)
        public int? OrderState { get; set; }
        public int? ExecutingIndex { get; set; }
        public double? Progress { get; set; }
        public string? ExecuteVehicleName { get; set; }
        public string? ExecuteVehicleKey { get; set; }
        public DateTime? LastSynced { get; set; }
        // -----------------------------------------------------------------------------------------------------------
        public int OrderProcessId { get; set; }
        public OrderProcess OrderProcess { get; set; } = default!;

    }
