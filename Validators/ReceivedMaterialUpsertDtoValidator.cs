using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class ReceivedMaterialUpsertDtoValidator : AbstractValidator<ReceivedMaterialUpsertDto>
{
    public ReceivedMaterialUpsertDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ 1. MaterialId: ต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.MaterialId)
            .GreaterThan(0)
            .WithMessage("MaterialId must be greater than 0. / รหัสวัสดุต้องมากกว่า 0");

        // ------------------------------------------------------------
        // ✅ 2. ReceivedQty: ต้อง ≥ 0
        // ------------------------------------------------------------
        RuleFor(x => x.ReceivedQty)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ReceivedQty must be greater than or equal to 0. / จำนวนที่รับต้องมากกว่าหรือเท่ากับ 0");
    }
}
