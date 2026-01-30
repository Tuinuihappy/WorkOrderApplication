using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class PreparingMaterialUpsertDtoValidator : AbstractValidator<PreparingMaterialUpsertDto>
{
    public PreparingMaterialUpsertDtoValidator()
    {
        // ------------------------------------------------------------
        // ✅ 1. MaterialId: ต้องมีค่าและต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.MaterialId)
            .GreaterThan(0)
            .WithMessage("MaterialId must be greater than 0. / รหัสวัสดุต้องมากกว่า 0");

        // ------------------------------------------------------------
        // ✅ 2. PreparedQty: ต้องมีค่าและต้องมากกว่า 0
        // ------------------------------------------------------------
        RuleFor(x => x.PreparedQty)
            .GreaterThan(0)
            .WithMessage("PreparedQty must be greater than 0. / จำนวนวัสดุที่เตรียมต้องมากกว่า 0");
    }
}
