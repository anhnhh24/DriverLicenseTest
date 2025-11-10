using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Common;
using Microsoft.EntityFrameworkCore;

public class UserStatisticService : IUserStatisticService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserStatisticService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<UserStatisticDto>> GetUserStatisticAsync(string userId)
    {
        try
        {
            var statistic = await _unitOfWork.UserStatistics.GetOneAsync(
                filter: s => s.UserId == userId,
                include: s => s.Include(x => x.User)
            );

            if (statistic == null)
                return ApiResponse<UserStatisticDto>.ErrorResponse("User statistics not found");

            var statisticDto = new UserStatisticDto
            {
                UserId = statistic.UserId,
                TotalExamsTaken = statistic.TotalExamsTaken,
                TotalExamsPassed = statistic.TotalExamsPassed,
                TotalExamsFailed = statistic.TotalExamsFailed,
                TotalQuestionsAnswered = statistic.TotalQuestionsAnswered,
                TotalCorrectAnswers = statistic.TotalCorrectAnswers,
                AccuracyRate = statistic.AccuracyRate,
                TotalLearningTime = statistic.TotalLearningTime,
                AverageScore = statistic.AverageScore,
                CreatedAt = statistic.CreatedAt,
                UpdatedAt = statistic.UpdatedAt
            };

            return ApiResponse<UserStatisticDto>.SuccessResponse(statisticDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserStatisticDto>.ErrorResponse($"Error retrieving user statistics: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserStatisticDto>> UpdateUserStatisticAsync(string userId)
    {
        try
        {
            var mockExams = await _unitOfWork.MockExams.GetListAsync(
                filter: m => m.UserId == userId && m.IsSubmitted,
                include: m => m.Include(x => x.LicenseType)
            );

            var statistic = await _unitOfWork.UserStatistics.GetOneAsync(
                filter: s => s.UserId == userId
            );

            if (statistic == null)
            {
                statistic = new UserStatistic
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserStatistics.AddAsync(statistic);
            }

            // Reset counters before recalculation
            statistic.TotalExamsTaken = mockExams.Count();
            statistic.TotalExamsPassed = mockExams.Count(m => m.PassStatus == "Passed");
            statistic.TotalExamsFailed = mockExams.Count(m => m.PassStatus == "Failed");
            statistic.TotalQuestionsAnswered = mockExams.Sum(m => m.TotalQuestions);
            statistic.TotalCorrectAnswers = mockExams.Sum(m => m.CorrectAnswers);
            statistic.TotalLearningTime = mockExams.Sum(m => m.TimeSpent ?? 0);

            // Calculate rates
            statistic.AccuracyRate = statistic.TotalQuestionsAnswered == 0 ? 0 :
                (decimal)statistic.TotalCorrectAnswers / statistic.TotalQuestionsAnswered * 100;


            statistic.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserStatistics.UpdateAsync(statistic);
            await _unitOfWork.SaveChangesAsync();

            return await GetUserStatisticAsync(userId);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserStatisticDto>.ErrorResponse($"Error updating user statistics: {ex.Message}");
        }
    }
}