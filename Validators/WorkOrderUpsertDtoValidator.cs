using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

// -------------------- ✅ Validator สำหรับ Create --------------------
public class WorkOrderCreateDtoValidator : AbstractValidator<WorkOrderCreateDto>
{
    public WorkOrderCreateDtoValidator()
    {
        RuleFor(x => x.Order)
            .NotEmpty().WithMessage("Order is required. / จำเป็นต้องระบุหมายเลขใบสั่งผลิต")
            .MaximumLength(100).WithMessage("Order must not exceed 100 characters. / หมายเลขใบสั่งผลิตต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.OrderType)
            .NotEmpty().WithMessage("OrderType is required.")
            .MaximumLength(50).WithMessage("OrderType must not exceed 50 characters.");

        RuleFor(x => x.Plant)
            .NotEmpty().WithMessage("Plant is required.")
            .MaximumLength(50).WithMessage("Plant must not exceed 50 characters.");

        RuleFor(x => x.Material)
            .NotEmpty().WithMessage("Material is required.")
            .MaximumLength(200).WithMessage("Material must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0. / จำนวนต้องมากกว่า 0");



        // ✅ Validate Materials ภายใน
        RuleForEach(x => x.Materials)
            .SetValidator(new MaterialCreateDtoValidator());
    }
}

// -------------------- ✅ Validator สำหรับ Update --------------------
public class WorkOrderUpdateDtoValidator : AbstractValidator<WorkOrderUpdateDto>
{
    public WorkOrderUpdateDtoValidator()
    {
        RuleFor(x => x.Order)
            .NotEmpty().WithMessage("Order is required. / จำเป็นต้องระบุหมายเลขใบสั่งผลิต")
            .MaximumLength(100).WithMessage("Order must not exceed 100 characters. / หมายเลขใบสั่งผลิตต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.OrderType)
            .NotEmpty().WithMessage("OrderType is required.")
            .MaximumLength(50).WithMessage("OrderType must not exceed 50 characters.");

        RuleFor(x => x.Plant)
            .NotEmpty().WithMessage("Plant is required.")
            .MaximumLength(50).WithMessage("Plant must not exceed 50 characters.");

        RuleFor(x => x.Material)
            .NotEmpty().WithMessage("Material is required.")
            .MaximumLength(200).WithMessage("Material must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0. / จำนวนต้องมากกว่า 0");



        // ✅ Validate Materials ภายใน
        RuleForEach(x => x.Materials)
            .SetValidator(new MaterialUpdateDtoValidator());
    }
}
