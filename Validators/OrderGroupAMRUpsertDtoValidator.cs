using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

/// <summary>
/// ✅ Validator สำหรับตรวจสอบความถูกต้องของข้อมูล OrderGroupAMR ก่อนบันทึก
/// ใช้กับ DTO: OrderGroupAMRUpsertDto
/// </summary>
public class OrderGroupAMRUpsertDtoValidator : AbstractValidator<OrderGroupAMRUpsertDto>
{
    public OrderGroupAMRUpsertDtoValidator()
    {
        // 🏁 ตรวจสอบ SourceStationId ต้องมากกว่า 0
        // ✅ EN: Source station ID must be greater than 0
        // ✅ TH: รหัสสถานีต้นทางต้องมากกว่า 0
        RuleFor(x => x.SourceStationId)
            .GreaterThan(0)
            .WithMessage("SourceStationId must be greater than 0 / รหัสสถานีต้นทางต้องมากกว่า 0");

        // 📍 ตรวจสอบ SourceStation ต้องไม่ว่าง และมีความยาวไม่เกิน 100 ตัวอักษร
        // ✅ EN: Source station name cannot be empty and must be less than or equal to 100 characters
        // ✅ TH: ชื่อสถานีต้นทางต้องไม่ว่าง และต้องไม่เกิน 100 ตัวอักษร
        RuleFor(x => x.SourceStation)
            .NotEmpty()
            .WithMessage("SourceStation cannot be empty / ชื่อสถานีต้นทางต้องไม่ว่าง")
            .MaximumLength(100)
            .WithMessage("SourceStation must not exceed 100 characters / ชื่อสถานีต้นทางต้องไม่เกิน 100 ตัวอักษร");

        // 🎯 ตรวจสอบ DestinationStationId ต้องมากกว่า 0
        // ✅ EN: Destination station ID must be greater than 0
        // ✅ TH: รหัสสถานีปลายทางต้องมากกว่า 0
        RuleFor(x => x.DestinationStationId)
            .GreaterThan(0)
            .WithMessage("DestinationStationId must be greater than 0 / รหัสสถานีปลายทางต้องมากกว่า 0");

        // 🏠 ตรวจสอบ DestinationStation ต้องไม่ว่าง และมีความยาวไม่เกิน 100 ตัวอักษร
        // ✅ EN: Destination station name cannot be empty and must be less than or equal to 100 characters
        // ✅ TH: ชื่อสถานีปลายทางต้องไม่ว่าง และต้องไม่เกิน 100 ตัวอักษร
        RuleFor(x => x.DestinationStation)
            .NotEmpty()
            .WithMessage("DestinationStation cannot be empty / ชื่อสถานีปลายทางต้องไม่ว่าง")
            .MaximumLength(100)
            .WithMessage("DestinationStation must not exceed 100 characters / ชื่อสถานีปลายทางต้องไม่เกิน 100 ตัวอักษร");

        // 🔗 ตรวจสอบ OrderGroupId ต้องมากกว่า 0
        // ✅ EN: OrderGroupId must be greater than 0
        // ✅ TH: รหัสกลุ่มคำสั่งต้องมากกว่า 0
        RuleFor(x => x.OrderGroupId)
            .GreaterThan(0)
            .WithMessage("OrderGroupId must be greater than 0 / รหัสกลุ่มคำสั่งต้องมากกว่า 0");

        // 🚫 ห้าม Source และ Destination เป็นจุดเดียวกัน (ตรวจจาก Id)
        // ✅ EN: Source and destination stations must not be the same (by ID)
        // ✅ TH: รหัสสถานีต้นทางและปลายทางต้องไม่เหมือนกัน
        RuleFor(x => x)
            .Must(x => x.SourceStationId != x.DestinationStationId)
            .WithMessage("Source and Destination stations must not be the same (by ID) / รหัสสถานีต้นทางและปลายทางต้องไม่เหมือนกัน");

        // 🚫 ห้าม Source และ Destination เป็นจุดเดียวกัน (ตรวจจากชื่อ)
        // ✅ EN: Source and destination station names must not be identical
        // ✅ TH: ชื่อสถานีต้นทางและปลายทางต้องไม่เหมือนกัน
        RuleFor(x => x)
            .Must(x => !string.Equals(x.SourceStation, x.DestinationStation, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Source and Destination station names must not be identical / ชื่อสถานีต้นทางและปลายทางต้องไม่เหมือนกัน");
    }
}
