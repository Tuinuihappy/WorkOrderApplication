using FluentValidation;
using WorkOrderApplication.API.Dtos;
using System.Linq;

namespace WorkOrderApplication.API.Validators
{
    // -------------------- ✅ PreparingProcessUpsertDtoValidator --------------------
    public class PreparingProcessUpsertDtoValidator : AbstractValidator<PreparingProcessUpsertDto>
    {
        public PreparingProcessUpsertDtoValidator()
        {
            // ------------------------------------------------------------
            // ✅ 1. PreparingByUserId ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.PreparingByUserId)
                .GreaterThan(0)
                .WithMessage("PreparingByUserId must be a positive integer. / รหัสผู้เตรียมต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ 2. OrderProcessId ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.OrderProcessId)
                .GreaterThan(0)
                .WithMessage("OrderProcessId must be a positive integer. / รหัสกระบวนการคำสั่งซื้อต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ 5. PreparingMaterials ต้องมีข้อมูล และไม่ว่าง
            // ------------------------------------------------------------
            RuleFor(x => x.PreparingMaterials)
                .NotNull().WithMessage("PreparingMaterials is required. / จำเป็นต้องระบุรายการวัสดุที่เตรียม")
                .NotEmpty().WithMessage("PreparingMaterials must contain at least 1 item. / ต้องมีวัสดุอย่างน้อย 1 รายการ");

            // ------------------------------------------------------------
            // ✅ 6. Validate วัสดุภายในแต่ละรายการ
            // ------------------------------------------------------------
            RuleForEach(x => x.PreparingMaterials)
                .SetValidator(new PreparingMaterialUpsertDtoValidator());

            // ------------------------------------------------------------
            // ✅ 7. ห้ามซ้ำ MaterialId ภายในรายการเดียวกัน
            // ------------------------------------------------------------
            RuleFor(x => x.PreparingMaterials)
                .Must(list =>
                {
                    if (list is null) return true;
                    return list.Select(i => i.MaterialId).Distinct().Count() == list.Count;
                })
                .WithMessage("PreparingMaterials contains duplicate MaterialId. / พบรายการวัสดุซ้ำ (MaterialId ซ้ำกัน)");

            // ------------------------------------------------------------
            // ✅ 8. จำกัดจำนวนรายการสูงสุด (≤ 500)
            // ------------------------------------------------------------
            RuleFor(x => x.PreparingMaterials.Count)
                .LessThanOrEqualTo(500)
                .WithMessage("PreparingMaterials must not exceed 500 items. / รายการวัสดุต้องไม่เกิน 500 รายการ");
        }
    }
}
