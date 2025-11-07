using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.UserStatistic;

namespace DriverLicenseTest.Application.Services.Interfaces;

public interface IUserStatisticService
{
    Task<ApiResponse<UserStatisticDto>> GetUserStatisticsAsync(string userId);

    Task<ApiResponse<bool>> UpdateStatisticsAfterExamAsync(
        string userId, int score, int correctAnswers, int totalQuestions, int timeSpent, bool isPassed);

    Task<ApiResponse<Dictionary<string, int>>> GetStatisticsByLicenseTypeAsync(string userId);
}
