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
            // ✅ MaterialDescription: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.MaterialDescription)
                .NotEmpty().WithMessage("MaterialDescription is required. / จำเป็นต้องระบุคำอธิบายวัสดุ");

            // ------------------------------------------------------------
            // ✅ ReqmntQty: ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.ReqmntQty)
                .GreaterThan(0).WithMessage("ReqmntQty must be greater than 0. / จำนวนวัสดุต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ BUn: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.BUn)
                .NotEmpty().WithMessage("BUn is required. / จำเป็นต้องระบุหน่วยของวัสดุ");
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
            // ✅ MaterialDescription: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.MaterialDescription)
                .NotEmpty().WithMessage("MaterialDescription is required. / จำเป็นต้องระบุคำอธิบายวัสดุ");

            // ------------------------------------------------------------
            // ✅ ReqmntQty: ต้องมากกว่า 0
            // ------------------------------------------------------------
            RuleFor(x => x.ReqmntQty)
                .GreaterThan(0).WithMessage("ReqmntQty must be greater than 0. / จำนวนวัสดุต้องมากกว่า 0");

            // ------------------------------------------------------------
            // ✅ BUn: ต้องกรอก
            // ------------------------------------------------------------
            RuleFor(x => x.BUn)
                .NotEmpty().WithMessage("BUn is required. / จำเป็นต้องระบุหน่วยของวัสดุ");
        }
    }
}
