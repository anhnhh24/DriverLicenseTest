using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.MockExam;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace DriverLicenseTest.Application.Services;

public class MockExamService : IMockExamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQuestionService _questionService;
    private readonly ExamStructuresOptions _examStructures;

    public MockExamService(
        IUnitOfWork unitOfWork,
        IQuestionService questionService,
        IOptions<ExamStructuresOptions> examStructures)
    {
        _unitOfWork = unitOfWork;
        _questionService = questionService;
        _examStructures = examStructures.Value;
    }

    public async Task<ApiResponse<MockExamDto>> StartMockExamAsync(string userId, string licenseType)
    {
        try
        {
            // Validate user
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<MockExamDto>.ErrorResponse("User not found");

            // Get exam structure from config
            var examStructure = _examStructures.ExamStructures
                .FirstOrDefault(x => x.LicenseType.Equals(licenseType));

            if (examStructure == null)
                return ApiResponse<MockExamDto>.ErrorResponse($"License type '{licenseType}' not found");

            // Get license type from database
            var licenseTypeEntity = await _unitOfWork.LicenseTypes.GetOneAsync(
                filter: lt => lt.LicenseCode.Equals(licenseType)
            );

            if (licenseTypeEntity == null)
                return ApiResponse<MockExamDto>.ErrorResponse($"License type '{licenseType}' not found in database");

            // Collect questions based on exam structure
            var examQuestions = new List<Question>();

            foreach (var categoryStructure in examStructure.Structure)
            {
                var categoryId = categoryStructure.Key;
                var requiredQuestionCount = categoryStructure.Value;

                // Get random questions from this category
                var questionResponse = await _questionService.GetRandomQuestionsByCategoryAsync(
                    categoryId, requiredQuestionCount);

                if (!questionResponse.Success)
                {
                    return ApiResponse<MockExamDto>.ErrorResponse(
                        $"Cannot get enough questions from category {categoryId}: {questionResponse.Message}");
                }

                var ids = questionResponse.Data.Select(d => d.QuestionId).ToList();

                var questionsEnumerable = await _unitOfWork.Questions.GetListAsync(
                    filter: q => ids.Contains(q.QuestionId),
                    include: q => q.Include(qt => qt.AnswerOptions).Include(qt => qt.Category)
                );

                var questions = questionsEnumerable.ToList();

                examQuestions.AddRange(questions);
            }

            // Shuffle questions for randomness
            var shuffledQuestions = examQuestions
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            // Create mock exam
            var mockExam = new MockExam
            {
                UserId = userId,
                LicenseTypeId = licenseTypeEntity.LicenseTypeId,
                TotalQuestions = examStructure.TotalQuestions,
                CorrectAnswers = 0,
                WrongAnswers = 0,
                PassingScore = examStructure.PassingScore,
                PassStatus = "InProgress",
                IsSubmitted = false,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MockExams.AddAsync(mockExam);
            await _unitOfWork.SaveChangesAsync();

            // Create mock exam answers for each question
            var mockExamAnswers = new List<MockExamAnswer>();
            foreach (var question in shuffledQuestions)
            {
                var mockExamAnswer = new MockExamAnswer
                {
                    ExamId = mockExam.ExamId,
                    QuestionId = question.QuestionId,
                    SelectedOptionId = null,
                    IsCorrect = false,
                    CreatedAt = DateTime.UtcNow
                };
                mockExamAnswers.Add(mockExamAnswer);
            }

            await _unitOfWork.MockExamAnswers.AddRangeAsync(mockExamAnswers);
            await _unitOfWork.SaveChangesAsync();

            // Map to DTO with questions
            var examDto = new MockExamDto
            {
                ExamId = mockExam.ExamId,
                UserId = mockExam.UserId,
                LicenseTypeId = mockExam.LicenseTypeId,
                TotalQuestions = mockExam.TotalQuestions,
                PassingScore = (int)mockExam.PassingScore,
                PassStatus = mockExam.PassStatus,
                IsSubmitted = mockExam.IsSubmitted,
                StartedAt = mockExam.StartedAt,
                CreatedAt = mockExam.CreatedAt
            };

            return ApiResponse<MockExamDto>.SuccessResponse(examDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MockExamDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get mock exam details with questions and answers (maps to existing DTOs)
    /// </summary>
    public async Task<ApiResponse<MockExamDto>> GetMockExamAsync(int examId)
    {
        try
        {
            var mockExam = await _unitOfWork.MockExams.GetOneAsync(
                filter: m => m.ExamId == examId,
                include: m => m
                    .Include(x => x.MockExamAnswers)
                        .ThenInclude(a => a.Question)
                            .ThenInclude(q => q.AnswerOptions)
                    .Include(x => x.MockExamAnswers)
                        .ThenInclude(a => a.Question)
                            .ThenInclude(q => q.Category)
                    .Include(x => x.MockExamAnswers)
                        .ThenInclude(a => a.SelectedOption)
                    .Include(x => x.LicenseType)
            );

            if (mockExam == null)
                return ApiResponse<MockExamDto>.ErrorResponse("Mock exam not found");

            var examDto = new MockExamDto
            {
                ExamId = mockExam.ExamId,
                UserId = mockExam.UserId,
                LicenseTypeId = mockExam.LicenseTypeId,
                LicenseTypeName = mockExam.LicenseType?.LicenseName ?? string.Empty,
                TotalQuestions = mockExam.TotalQuestions,
                CorrectAnswers = mockExam.CorrectAnswers,
                WrongAnswers = mockExam.WrongAnswers,
                PassingScore = mockExam.PassingScore,
                PassStatus = mockExam.PassStatus,
                IsSubmitted = mockExam.IsSubmitted,
                TimeSpent = mockExam.TimeSpent ?? 0,
                StartedAt = mockExam.StartedAt,
                CompletedAt = mockExam.CompletedAt,
                CreatedAt = mockExam.CreatedAt
            };

            // Map answers -> Questions DTO (use existing DTO shape)
            examDto.Questions = mockExam.MockExamAnswers
                .OrderBy(a => a.ExamAnswerId)
                .Select(a =>
                {
                    var q = a.Question;
                    var answerOptions = q?.AnswerOptions?
                        .OrderBy(o => o.OptionOrder)
                        .Select(o => new AnswerOptionDto
                        {
                            OptionId = o.OptionId,
                            OptionText = o.OptionText,
                            // Only reveal correctness after submission: mark selected option correctness when submitted
                            IsCorrect = mockExam.IsSubmitted && a.SelectedOptionId.HasValue && a.SelectedOptionId.Value == o.OptionId
                                ? a.IsCorrect
                                : false
                        })
                        .ToList() ?? new List<AnswerOptionDto>();

                    return new MockExamQuestionDto
                    {
                        QuestionId = a.QuestionId,
                        QuestionNumber = q?.QuestionNumber ?? 0,
                        QuestionText = q?.QuestionText ?? string.Empty,
                        CategoryId = q?.CategoryId ?? 0,
                        CategoryName = q?.Category?.CategoryName ?? string.Empty,
                        AnswerOptions = answerOptions,
                        SelectedOptionId = a.SelectedOptionId,
                        IsCorrect = a.IsCorrect
                    };
                })
                .ToList();

            return ApiResponse<MockExamDto>.SuccessResponse(examDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MockExamDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Submit mock exam and calculate results
    /// </summary>
    public async Task<ApiResponse<MockExamDto>> SubmitMockExamAsync(int examId)
    {
        try
        {
            var mockExam = await _unitOfWork.MockExams.GetOneAsync(
                filter: m => m.ExamId == examId,
                include: m => m
                    .Include(x => x.MockExamAnswers)
                        .ThenInclude(a => a.SelectedOption)
                    .Include(x => x.LicenseType)
            );

            if (mockExam == null)
                return ApiResponse<MockExamDto>.ErrorResponse("Mock exam not found");

            if (mockExam.IsSubmitted)
                return ApiResponse<MockExamDto>.ErrorResponse("Mock exam already submitted");

            // Calculate results
            var mockExamAnswers = mockExam.MockExamAnswers.ToList();
            var correctCount = 0;
            var wrongCount = 0;

            foreach (var answer in mockExamAnswers)
            {
                if (answer.IsCorrect)
                    correctCount++;
                else
                    wrongCount++;
            }

            // Determine pass/fail
            mockExam.CorrectAnswers = correctCount;
            mockExam.WrongAnswers = wrongCount;
            mockExam.Score = correctCount;
            mockExam.PassStatus = correctCount >= mockExam.PassingScore ? "Passed" : "Failed";
            mockExam.IsSubmitted = true;
            mockExam.CompletedAt = DateTime.UtcNow;
            mockExam.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.MockExams.UpdateAsync(mockExam);

            // Update user statistics
            await UpdateUserStatisticsAsync(mockExam.UserId, correctCount >= mockExam.PassingScore);

            await _unitOfWork.SaveChangesAsync();

            var examDto = new MockExamDto
            {
                ExamId = mockExam.ExamId,
                UserId = mockExam.UserId,
                LicenseTypeId = mockExam.LicenseTypeId,
                TotalQuestions = mockExam.TotalQuestions,
                CorrectAnswers = mockExam.CorrectAnswers,
                WrongAnswers = mockExam.WrongAnswers,
                Score = mockExam.Score,
                PassingScore = (int)mockExam.PassingScore,
                PassStatus = mockExam.PassStatus,
                IsSubmitted = mockExam.IsSubmitted,
                CompletedAt = mockExam.CompletedAt
            };

            return ApiResponse<MockExamDto>.SuccessResponse(examDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MockExamDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Private method: Update user statistics after exam
    /// </summary>
    private async Task UpdateUserStatisticsAsync(string userId, bool isPassed)
    {
        var statistic = await _unitOfWork.UserStatistics.GetOneAsync(
            filter: s => s.UserId == userId
        );

        if (statistic != null)
        {
            statistic.TotalExamsTaken++;
            if (isPassed)
                statistic.TotalExamsPassed++;
            else
                statistic.TotalExamsFailed++;

            statistic.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserStatistics.UpdateAsync(statistic);
        }
    }
}