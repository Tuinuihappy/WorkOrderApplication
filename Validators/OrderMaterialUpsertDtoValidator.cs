using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators
{
    public class OrderMaterialUpsertDtoValidator : AbstractValidator<OrderMaterialUpsertDto>
    {
        public OrderMaterialUpsertDtoValidator()
        {
            // ------------------------------------------------------------
            // ✅ 1. MaterialId: ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.MaterialId)
                .GreaterThan(0)
                .WithMessage("MaterialId must be greater than 0. / รหัสวัสดุต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ 2. OrderQty: ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.OrderQty)
                .GreaterThan(0)
                .WithMessage("Order quantity must be greater than 0. / จำนวนที่สั่งต้องมากกว่า 0");
        }
    }
}
