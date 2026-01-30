using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

// -------------------- ✅ Validator สำหรับ Create --------------------
public class WorkOrderCreateDtoValidator : AbstractValidator<WorkOrderCreateDto>
{
    public WorkOrderCreateDtoValidator()
    {
        RuleFor(x => x.WorkOrderNumber)
            .NotEmpty().WithMessage("WorkOrderNumber is required. / จำเป็นต้องระบุหมายเลขใบสั่งผลิต")
            .MaximumLength(100).WithMessage("WorkOrderNumber must not exceed 100 characters. / หมายเลขใบสั่งผลิตต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.LineName)
            .NotEmpty().WithMessage("LineName is required. / จำเป็นต้องระบุชื่อไลน์ผลิต")
            .MaximumLength(100).WithMessage("LineName must not exceed 100 characters. / ชื่อไลน์ผลิตต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.ModelName)
            .NotEmpty().WithMessage("ModelName is required. / จำเป็นต้องระบุชื่อโมเดล")
            .MaximumLength(100).WithMessage("ModelName must not exceed 100 characters. / ชื่อโมเดลต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0. / จำนวนต้องมากกว่า 0");

        RuleFor(x => x.CreatedByUserId)
            .GreaterThan(0).WithMessage("CreatedByUserId must be a valid user id. / ผู้สร้างต้องมีรหัสผู้ใช้ที่ถูกต้อง");

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
        RuleFor(x => x.WorkOrderNumber)
            .NotEmpty().WithMessage("WorkOrderNumber is required. / จำเป็นต้องระบุหมายเลขใบสั่งผลิต")
            .MaximumLength(100).WithMessage("WorkOrderNumber must not exceed 100 characters. / หมายเลขใบสั่งผลิตต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.LineName)
            .NotEmpty().WithMessage("LineName is required. / จำเป็นต้องระบุชื่อไลน์ผลิต")
            .MaximumLength(100).WithMessage("LineName must not exceed 100 characters. / ชื่อไลน์ผลิตต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.ModelName)
            .NotEmpty().WithMessage("ModelName is required. / จำเป็นต้องระบุชื่อโมเดล")
            .MaximumLength(100).WithMessage("ModelName must not exceed 100 characters. / ชื่อโมเดลต้องไม่เกิน 100 ตัวอักษร");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0. / จำนวนต้องมากกว่า 0");

        RuleFor(x => x.UpdatedByUserId)
            .GreaterThan(0).When(x => x.UpdatedByUserId.HasValue)
            .WithMessage("UpdatedByUserId must be a valid user id. / ผู้แก้ไขต้องมีรหัสผู้ใช้ที่ถูกต้อง");

        // ✅ Validate Materials ภายใน
        RuleForEach(x => x.Materials)
            .SetValidator(new MaterialUpdateDtoValidator());
    }
}
