using WorkOrderApplication.API.Entities;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Extensions;

namespace WorkOrderApplication.API.Mappings;

public static class UserMapping
{
    // -------------------- Entity → DTO --------------------
    public static UserDetailsDto ToDetailsDto(this User user)
    {
        return new UserDetailsDto(
            user.Id,
            user.UserName,
            user.EmployeeId,
            user.Position,
            user.Department,
            user.Shift,
            user.ContactNumber,
            user.Email,
            user.CreatedDate.ToICT(),
            user.UpdatedDate.ToICT()
        );
    }

    public static UserListDto ToListDto(this User user)
    {
        return new UserListDto(
            user.Id,
            user.UserName,
            user.EmployeeId,
            user.Position,
            user.Department,
            user.Shift
        );
    }

    // -------------------- DTO → Entity --------------------
    public static User ToEntity(this UserUpsertDto dto)
    {
        return new User
        {
            UserName = dto.UserName,
            EmployeeId = dto.EmployeeId,
            Position = dto.Position,
            Department = dto.Department,
            Shift = dto.Shift,
            ContactNumber = dto.ContactNumber,
            Email = dto.Email,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    // -------------------- Update Entity จาก DTO --------------------
    public static void UpdateEntity(this User user, UserUpsertDto dto)
    {
        user.UserName = dto.UserName;
        user.EmployeeId = dto.EmployeeId;
        user.Position = dto.Position;
        user.Department = dto.Department;
        user.Shift = dto.Shift;
        user.ContactNumber = dto.ContactNumber;
        user.Email = dto.Email;
        user.UpdatedDate = DateTime.UtcNow;
    }
}
