using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class ReceivedProcessUpsertDtoValidator : AbstractValidator<ReceivedProcessUpsertDto>
{
    public ReceivedProcessUpsertDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ 1. ReceivedByUserId: ต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.ReceivedByUserId)
            .GreaterThan(0)
            .WithMessage("ReceivedByUserId must be greater than 0. / รหัสผู้รับต้องมากกว่า 0");

        // ------------------------------------------------------------
        // ✅ 2. OrderProcessId: ต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.OrderProcessId)
            .GreaterThan(0)
            .WithMessage("OrderProcessId must be greater than 0. / รหัสกระบวนการคำสั่งซื้อต้องมากกว่า 0");

        // ------------------------------------------------------------
        // ✅ 3. ReceivedMaterials: ต้องตรวจสอบภายในแต่ละรายการ
        // ------------------------------------------------------------
        RuleForEach(x => x.ReceivedMaterials)
            .SetValidator(new ReceivedMaterialUpsertDtoValidator());

        // ------------------------------------------------------------
        // ✅ 4. ReceivedMaterials ต้องไม่ว่าง
        // ------------------------------------------------------------
        RuleFor(x => x.ReceivedMaterials)
            .NotNull().WithMessage("ReceivedMaterials is required. / จำเป็นต้องระบุรายการวัสดุที่รับเข้า")
            .NotEmpty().WithMessage("ReceivedMaterials must contain at least 1 item. / ต้องมีวัสดุที่รับเข้าอย่างน้อย 1 รายการ");

        // ------------------------------------------------------------
        // ✅ 5. ห้ามซ้ำ MaterialId ภายในรายการเดียวกัน
        // ------------------------------------------------------------
        RuleFor(x => x.ReceivedMaterials)
            .Must(list =>
            {
                if (list is null) return true;
                return list.Select(i => i.MaterialId).Distinct().Count() == list.Count;
            })
            .WithMessage("ReceivedMaterials contains duplicate MaterialId. / พบรายการวัสดุซ้ำ (MaterialId ซ้ำกัน)");

        // ------------------------------------------------------------
        // ✅ 6. จำกัดจำนวนสูงสุด 500 รายการ
        // ------------------------------------------------------------
        RuleFor(x => x.ReceivedMaterials.Count)
            .LessThanOrEqualTo(500)
            .WithMessage("ReceivedMaterials must not exceed 500 items. / รายการวัสดุต้องไม่เกิน 500 รายการ");
    }
}
