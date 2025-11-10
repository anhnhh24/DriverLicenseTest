using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.MockExam;
using Microsoft.EntityFrameworkCore;

public class UserWrongQuestionService : IUserWrongQuestionService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserWrongQuestionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<UserWrongQuestionDto>>> GetUserWrongQuestionsAsync(string userId)
    {
        try
        {
            var wrongQuestions = await _unitOfWork.UserWrongQuestions.GetListAsync(
                filter: w => w.UserId == userId,
                include: q => q
                    .Include(w => w.Question)
                        .ThenInclude(q => q.AnswerOptions)
                    .Include(w => w.Question)
                        .ThenInclude(q => q.Category),
                orderBy: q => q.OrderByDescending(w => w.LastWrongAt)
            );

            var dtos = wrongQuestions.Select(wq => MapToDto(wq)).ToList();
            return ApiResponse<IEnumerable<UserWrongQuestionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<UserWrongQuestionDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<UserWrongQuestionDto>>> GetUserWrongQuestionsByLicenseAsync(int licenseTypeId, string userId)
    {
        try
        {
            var wrongQuestions = await _unitOfWork.UserWrongQuestions.GetListAsync(
                filter: w => w.UserId == userId &&
                            w.Question.LicenseQuestions.Any(lq => lq.LicenseTypeId == licenseTypeId),
                include: q => q
                    .Include(w => w.Question)
                        .ThenInclude(q => q.AnswerOptions)
                    .Include(w => w.Question)
                        .ThenInclude(q => q.Category)
                    .Include(w => w.Question)
                        .ThenInclude(q => q.LicenseQuestions)
                            .ThenInclude(lq => lq.LicenseType),
                orderBy: q => q.OrderByDescending(w => w.LastWrongAt)
            );

            if (!wrongQuestions.Any())
                return ApiResponse<IEnumerable<UserWrongQuestionDto>>.SuccessResponse(
                    new List<UserWrongQuestionDto>(),
                    "No wrong questions found for this license type"
                );

            var dtos = wrongQuestions.Select(wq => MapToDto(wq)).ToList();
            return ApiResponse<IEnumerable<UserWrongQuestionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<UserWrongQuestionDto>>.ErrorResponse(
                $"Error retrieving wrong questions: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<bool>> MarkQuestionAsFixedAsync(int wrongQuestionId, string userId)
    {
        try
        {
            var wrongQuestion = await _unitOfWork.UserWrongQuestions.GetOneAsync(
                filter: w => w.WrongQuestionId == wrongQuestionId && w.UserId == userId
            );

            if (wrongQuestion == null)
                return ApiResponse<bool>.ErrorResponse("Wrong question not found");

            wrongQuestion.IsFixed = true;
            wrongQuestion.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserWrongQuestions.UpdateAsync(wrongQuestion);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    private static UserWrongQuestionDto MapToDto(UserWrongQuestion wq)
    {
        var correctOption = wq.Question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);

        return new UserWrongQuestionDto
        {
            UserWrongQuestionId = wq.WrongQuestionId,
            UserId = wq.UserId,
            QuestionId = wq.QuestionId,
            WrongCount = wq.WrongCount,
            IsFixed = wq.IsFixed,
            LastWrongAt = wq.LastWrongAt,
            QuestionText = wq.Question.QuestionText,
            ImageUrl = wq.Question.ImageUrl,
            CategoryId = wq.Question.CategoryId,
            CategoryName = wq.Question.Category.CategoryName,
            AnswerOptions = wq.Question.AnswerOptions
                .OrderBy(o => o.OptionOrder)
                .Select(o => new AnswerOptionDto
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList(),
            CorrectOptionId = correctOption?.OptionId,
            CorrectAnswerText = correctOption?.OptionText
        };
    }
}