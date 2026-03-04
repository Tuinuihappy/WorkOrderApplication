using FluentValidation;
using WorkOrderApplication.API.Dtos;

namespace WorkOrderApplication.API.Validators;

public class UserUpsertDtoValidator : AbstractValidator<UserUpsertDto>
{
    public UserUpsertDtoValidator()
    {
        // -------------------- ✅ UserName --------------------
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required. / จำเป็นต้องระบุชื่อผู้ใช้")
            .MaximumLength(100).WithMessage("UserName must not exceed 100 characters. / ชื่อผู้ใช้ต้องไม่เกิน 100 ตัวอักษร");

        // -------------------- ✅ EmployeeId --------------------
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId is required. / จำเป็นต้องระบุรหัสพนักงาน")
            .MaximumLength(50).WithMessage("EmployeeId must not exceed 50 characters. / รหัสพนักงานต้องไม่เกิน 50 ตัวอักษร");

        // -------------------- ✅ Position --------------------
        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required. / จำเป็นต้องระบุตำแหน่ง")
            .MaximumLength(50).WithMessage("Position must not exceed 50 characters. / ตำแหน่งต้องไม่เกิน 50 ตัวอักษร");

        // -------------------- ✅ Shift --------------------
        RuleFor(x => x.Shift)
            .NotEmpty().WithMessage("Shift is required. / จำเป็นต้องระบุรอบการทำงาน (กะ)")
            .MaximumLength(20).WithMessage("Shift must not exceed 20 characters. / รอบการทำงานต้องไม่เกิน 20 ตัวอักษร");

        // -------------------- ✅ Role --------------------
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required. / จำเป็นต้องระบุสิทธิ์การใช้งาน")
            .MaximumLength(50).WithMessage("Role must not exceed 50 characters. / สิทธิ์การใช้งานต้องไม่เกิน 50 ตัวอักษร");

        // -------------------- ✅ Password (Optional for Update) --------------------
        RuleFor(x => x.Password)
            .MinimumLength(6).When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must be at least 6 characters. / รหัสผ่านต้องมีความยาวอย่างน้อย 6 ตัวอักษร");
    }
}
