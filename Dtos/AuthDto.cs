namespace WorkOrderApplication.API.Dtos;

public record LoginRequestDto(
    string EmployeeId,
    string Password
);

public record LoginResponseDto(
    string Token,
    UserDetailsDto UserInfo
);
