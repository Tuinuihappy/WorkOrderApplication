using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators
{
    // -------------------- ✅ สำหรับการสร้างวัสดุใหม่ (Create) --------------------
    public class MaterialCreateDtoValidator : AbstractValidator<MaterialCreateDto>
    {
        public MaterialCreateDtoValidator()
        {
            // ------------------------------------------------------------
            // ✅ MaterialNumber: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.MaterialNumber)
                .NotEmpty().WithMessage("MaterialNumber is required. / จำเป็นต้องระบุหมายเลขวัสดุ");

            // ------------------------------------------------------------
            // ✅ Description: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required. / จำเป็นต้องระบุคำอธิบายวัสดุ");

            // ------------------------------------------------------------
            // ✅ Quantity: ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0. / จำนวนวัสดุต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ RequestPerHour: ต้อง ≥ 0 (อนุญาตให้เป็นศูนย์ได้)
            // ------------------------------------------------------------
            RuleFor(x => x.RequestPerHour)
                .GreaterThanOrEqualTo(0).WithMessage("RequestPerHour must be greater than or equal to 0. / อัตราการขอวัสดุต่อชั่วโมงต้องมากกว่าหรือเท่ากับ 0");

            // ------------------------------------------------------------
            // ✅ Unit: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required. / จำเป็นต้องระบุหน่วยของวัสดุ");
        }
    }

    // -------------------- ✅ สำหรับการอัปเดตวัสดุ (Update) --------------------
    public class MaterialUpdateDtoValidator : AbstractValidator<MaterialUpdateDto>
    {
        public MaterialUpdateDtoValidator()
        {
            // ------------------------------------------------------------
            // ✅ MaterialNumber: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.MaterialNumber)
                .NotEmpty().WithMessage("MaterialNumber is required. / จำเป็นต้องระบุหมายเลขวัสดุ");

            // ------------------------------------------------------------
            // ✅ Description: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required. / จำเป็นต้องระบุคำอธิบายวัสดุ");

            // ------------------------------------------------------------
            // ✅ Quantity: ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0. / จำนวนวัสดุต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ RequestPerHour: ต้อง ≥ 0
            // ------------------------------------------------------------
            RuleFor(x => x.RequestPerHour)
                .GreaterThanOrEqualTo(0).WithMessage("RequestPerHour must be greater than or equal to 0. / อัตราการขอวัสดุต่อชั่วโมงต้องมากกว่าหรือเท่ากับ 0");

            // ------------------------------------------------------------
            // ✅ Unit: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required. / จำเป็นต้องระบุหน่วยของวัสดุ");
        }
    }
}
