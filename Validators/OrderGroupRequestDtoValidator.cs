using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class OrderGroupRequestDtoValidator : AbstractValidator<OrderGroupRequestDto>
{
    public OrderGroupRequestDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ OrderGroupId: ต้องมีค่า > 0
        // ------------------------------------------------------------
        RuleFor(x => x.OrderGroupId)
            .GreaterThan(0)
            .WithMessage("OrderGroupId must be greater than 0. / รหัสกลุ่มคำสั่ง (OrderGroupId) ต้องมากกว่า 0");
    }
}
