using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class CancelledProcessUpsertDtoValidator : AbstractValidator<CancelledProcessUpsertDto>
{
    public CancelledProcessUpsertDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ 1. Reason: ต้องระบุเหตุผล และไม่เกิน 200 ตัวอักษร
        // ------------------------------------------------------------
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required. / จำเป็นต้องระบุเหตุผลในการยกเลิก")
            .MaximumLength(200)
            .WithMessage("Reason must not exceed 200 characters. / เหตุผลในการยกเลิกต้องไม่เกิน 200 ตัวอักษร");

        // ------------------------------------------------------------
        // ✅ 2. OrderProcessId: ต้องมีค่า > 0
        // ------------------------------------------------------------
        RuleFor(x => x.OrderProcessId)
            .GreaterThan(0)
            .WithMessage("OrderProcessId must be greater than 0. / รหัสกระบวนการคำสั่งซื้อต้องมากกว่า 0");
    }
}
