using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class ReturnProcessUpsertDtoValidator : AbstractValidator<ReturnProcessUpsertDto>
{
    public ReturnProcessUpsertDtoValidator()
    {

        // ------------------------------------------------------------
        // ✅ 2. Reason: ต้องระบุ และต้องไม่เกิน 200 ตัวอักษร
        // ------------------------------------------------------------
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required. / จำเป็นต้องระบุเหตุผลในการคืนสินค้า")
            .MaximumLength(200)
            .WithMessage("Reason must not exceed 200 characters. / เหตุผลในการคืนสินค้าต้องไม่เกิน 200 ตัวอักษร");

        // ------------------------------------------------------------
        // ✅ 4. ReturnByUserId: ต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.ReturnByUserId)
            .GreaterThan(0)
            .WithMessage("ReturnByUserId must be greater than 0. / รหัสผู้คืนสินค้าต้องมากกว่า 0");

        // ------------------------------------------------------------
        // ✅ 5. OrderProcessId: ต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.OrderProcessId)
            .GreaterThan(0)
            .WithMessage("OrderProcessId must be greater than 0. / รหัสกระบวนการคำสั่งซื้อต้องมากกว่า 0");
    }
}
