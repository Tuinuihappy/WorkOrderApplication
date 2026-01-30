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

        // -------------------- ✅ Department --------------------
        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required. / จำเป็นต้องระบุแผนก")
            .MaximumLength(100).WithMessage("Department must not exceed 100 characters. / แผนกต้องไม่เกิน 100 ตัวอักษร");

        // -------------------- ✅ Shift --------------------
        RuleFor(x => x.Shift)
            .NotEmpty().WithMessage("Shift is required. / จำเป็นต้องระบุรอบการทำงาน (กะ)")
            .MaximumLength(20).WithMessage("Shift must not exceed 20 characters. / รอบการทำงานต้องไม่เกิน 20 ตัวอักษร");

        // -------------------- ✅ ContactNumber (optional + format) --------------------
        RuleFor(x => x.ContactNumber)
            .MaximumLength(20).WithMessage("ContactNumber must not exceed 20 characters. / เบอร์โทรศัพท์ต้องไม่เกิน 20 ตัวอักษร")
            .Matches(@"^[0-9+\-() ]*$")
            .When(x => !string.IsNullOrEmpty(x.ContactNumber))
            .WithMessage("ContactNumber contains invalid characters. / เบอร์โทรศัพท์มีอักขระที่ไม่ถูกต้อง");

        // -------------------- ✅ Email --------------------
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required. / จำเป็นต้องระบุอีเมล")
            .EmailAddress().WithMessage("Email format is invalid. / รูปแบบอีเมลไม่ถูกต้อง")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters. / อีเมลต้องไม่เกิน 150 ตัวอักษร");
    }
}
