using DriverLicenseTest.Shared.DTOs.Common;

public interface IUserStatisticService
{
    Task<ApiResponse<UserStatisticDto>> GetUserStatisticAsync(string userId);
    Task<ApiResponse<UserStatisticDto>> UpdateUserStatisticAsync(string userId);
}