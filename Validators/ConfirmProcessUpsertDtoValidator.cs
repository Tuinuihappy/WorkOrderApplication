using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class ConfirmProcessUpsertDtoValidator : AbstractValidator<ConfirmProcessUpsertDto>
{
    public ConfirmProcessUpsertDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ OrderProcessId: ต้องมีค่า และต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.OrderProcessId)
            .GreaterThan(0)
            .WithMessage("OrderProcessId is required and must be greater than 0. / ต้องระบุรหัสกระบวนการคำสั่งซื้อ (OrderProcessId) และต้องมีค่ามากกว่า 0");
    }
}
