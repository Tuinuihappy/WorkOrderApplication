using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class OrderProcessUpsertDtoValidator : AbstractValidator<OrderProcessUpsertDto>
{
    public OrderProcessUpsertDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ 1. OrderNumber: ต้องกรอก และไม่เกิน 50 ตัวอักษร
        // ------------------------------------------------------------
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("OrderNumber is required. / จำเป็นต้องระบุหมายเลขคำสั่งซื้อ (OrderNumber)")
            .MaximumLength(50).WithMessage("OrderNumber must not exceed 50 characters. / หมายเลขคำสั่งซื้อต้องไม่เกิน 50 ตัวอักษร");

        // ------------------------------------------------------------
        // ✅ 2. WorkOrderId: ต้องมีค่า > 0
        // ------------------------------------------------------------
        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0).WithMessage("WorkOrderId must be a valid id. / รหัสใบสั่งงานต้องมากกว่า 0 และต้องถูกต้อง");

        // ------------------------------------------------------------
        // ✅ 3. CreatedByUserId: ต้องมีค่า > 0
        // ------------------------------------------------------------
        RuleFor(x => x.CreatedByUserId)
            .GreaterThan(0).WithMessage("CreatedByUserId must be a valid id. / รหัสผู้สร้างต้องมากกว่า 0 และต้องถูกต้อง");
    }
}
