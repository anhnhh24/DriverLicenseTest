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
                CreatedAt = DateTime.UtcNow,
                TimeLimit = licenseTypeEntity.TimeLimit
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
                CreatedAt = mockExam.CreatedAt,
                TimeLimit = licenseTypeEntity.TimeLimit,
                Questions = mockExamAnswers.Select(a =>
                    new MockExamQuestionDto
                    {
                        QuestionId = a.QuestionId,
                        QuestionNumber = shuffledQuestions.FirstOrDefault(q => q.QuestionId == a.QuestionId)?.QuestionNumber ?? 0,
                        QuestionText = shuffledQuestions.FirstOrDefault(q => q.QuestionId == a.QuestionId)?.QuestionText ?? string.Empty,
                        CategoryId = shuffledQuestions.FirstOrDefault(q => q.QuestionId == a.QuestionId)?.CategoryId ?? 0,
                        CategoryName = shuffledQuestions.FirstOrDefault(q => q.QuestionId == a.QuestionId)?.Category?.CategoryName ?? string.Empty,
                        AnswerOptions = shuffledQuestions.FirstOrDefault(q => q.QuestionId == a.QuestionId)?.AnswerOptions?
                            .OrderBy(o => o.OptionOrder)
                            .Select(o => new AnswerOptionDto
                            {
                                OptionId = o.OptionId,
                                OptionText = o.OptionText,
                                IsCorrect = false
                            }).ToList() ?? new List<AnswerOptionDto>(),
                        SelectedOptionId = null,
                        IsCorrect = false,
                        ImageUrl = shuffledQuestions.FirstOrDefault(q => q.QuestionId == a.QuestionId)?.ImageUrl
                    }
                ).ToList()
            };

            return ApiResponse<MockExamDto>.SuccessResponse(examDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MockExamDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

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
                TimeLimit = mockExam.TimeLimit ?? 0,
                StartedAt = mockExam.StartedAt,
                CompletedAt = mockExam.CompletedAt,
                CreatedAt = mockExam.CreatedAt
            };

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
                        IsCorrect = a.IsCorrect,
                        ImageUrl = q?.ImageUrl
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
    public async Task<ApiResponse<MockExamDto>> SubmitMockExamAsync(int examId, SubmitExamRequest request)
    {
        try
        {
            var mockExam = await _unitOfWork.MockExams.GetOneAsync(
                filter: m => m.ExamId == request.ExamId,
                include: m => m
                    .Include(x => x.MockExamAnswers)
                        .ThenInclude(a => a.Question)
                            .ThenInclude(q => q.AnswerOptions)
                    .Include(x => x.LicenseType)
            );

            if (mockExam == null)
                return ApiResponse<MockExamDto>.ErrorResponse("Mock exam not found");

            if (mockExam.IsSubmitted)
                return ApiResponse<MockExamDto>.ErrorResponse("Exam already submitted");

            int correctCount = 0;
            int wrongCount = 0;

            foreach (var submission in request.Answers)
            {
                var examAnswer = mockExam.MockExamAnswers
                    .FirstOrDefault(a => a.QuestionId == submission.QuestionId);

                if (examAnswer != null)
                {
                    var existingWrongQuestion = await _unitOfWork.UserWrongQuestions.GetOneAsync(
                             filter: w => w.UserId == mockExam.UserId &&
                                        w.QuestionId == submission.QuestionId
                    );

                    examAnswer.SelectedOptionId = submission.SelectedOptionId;
                    examAnswer.UpdatedAt = DateTime.UtcNow;
                    examAnswer.ReviewedAfterSubmit = false;

                    var correctOption = examAnswer.Question.AnswerOptions
                        .FirstOrDefault(o => o.IsCorrect);

                    if (correctOption != null)
                    {
                        examAnswer.IsCorrect = submission.SelectedOptionId == correctOption.OptionId;
                        if (examAnswer.IsCorrect)
                        {
                            correctCount++;
                            if (existingWrongQuestion != null)
                            {
                                existingWrongQuestion.WrongCount--;
                                existingWrongQuestion.UpdatedAt = DateTime.UtcNow;
                                existingWrongQuestion.IsFixed = true;
                                await _unitOfWork.UserWrongQuestions.UpdateAsync(existingWrongQuestion);
                            }
                        }
                        else
                        {
                            wrongCount++;

                            if (existingWrongQuestion != null)
                            {
                                existingWrongQuestion.WrongCount++;
                                existingWrongQuestion.LastWrongAt = DateTime.UtcNow;
                                existingWrongQuestion.UpdatedAt = DateTime.UtcNow;
                                existingWrongQuestion.IsFixed = false;
                                await _unitOfWork.UserWrongQuestions.UpdateAsync(existingWrongQuestion);
                            }
                            else
                            {
                                var newWrongQuestion = new UserWrongQuestion
                                {
                                    UserId = mockExam.UserId,
                                    QuestionId = submission.QuestionId,
                                    WrongCount = 1,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow,
                                    LastWrongAt = DateTime.UtcNow,
                                    IsFixed = false
                                };
                                await _unitOfWork.UserWrongQuestions.AddAsync(newWrongQuestion);
                            }
                        }
                    }
                    await _unitOfWork.MockExamAnswers.UpdateAsync(examAnswer);
                }
            }

            mockExam.CorrectAnswers = correctCount;
            mockExam.WrongAnswers = wrongCount;
            mockExam.Score = correctCount;
            mockExam.TimeSpent = request.TimeSpent;
            mockExam.PassStatus = correctCount >= mockExam.PassingScore ? "Passed" : "Failed";
            mockExam.IsSubmitted = true;
            mockExam.CompletedAt = DateTime.UtcNow;
            mockExam.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.MockExams.UpdateAsync(mockExam);
            await UpdateUserStatisticsAsync(mockExam.UserId, mockExam.PassStatus == "Passed");
            await _unitOfWork.SaveChangesAsync();

            var examDto = new MockExamDto
            {
                ExamId = mockExam.ExamId,
                UserId = mockExam.UserId,
                LicenseTypeId = mockExam.LicenseTypeId,
                LicenseTypeName = mockExam.LicenseType?.LicenseName ?? string.Empty,
                TotalQuestions = mockExam.TotalQuestions,
                CorrectAnswers = mockExam.CorrectAnswers,
                WrongAnswers = mockExam.WrongAnswers,
                Score = mockExam.Score,
                PassingScore = mockExam.PassingScore,
                PassStatus = mockExam.PassStatus,
                IsSubmitted = mockExam.IsSubmitted,
                TimeSpent = mockExam.TimeSpent ?? 0,
                TimeLimit = mockExam.TimeLimit ?? 0,
                StartedAt = mockExam.StartedAt,
                CompletedAt = mockExam.CompletedAt,
                CreatedAt = mockExam.CreatedAt
            };

            return ApiResponse<MockExamDto>.SuccessResponse(examDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MockExamDto>.ErrorResponse($"Error submitting exam: {ex.Message}");
        }
    }
    public async Task<ApiResponse<MockExamDto>> UpdateMockExamAsync(int examId, string userId, MockExamDto updateDto)
    {
        try
        {
            var mockExam = await _unitOfWork.MockExams.GetOneAsync(
                filter: m => m.ExamId == examId,
                include: m => m
                    .Include(x => x.MockExamAnswers)
                    .Include(x => x.LicenseType)
            );

            if (mockExam == null)
                return ApiResponse<MockExamDto>.ErrorResponse("Mock exam not found");

            if (mockExam.UserId != userId)
                return ApiResponse<MockExamDto>.ErrorResponse("Unauthorized to update this exam");

            if (mockExam.IsSubmitted)
                return ApiResponse<MockExamDto>.ErrorResponse("Cannot update submitted exam");

            // Update allowed fields
            mockExam.TimeSpent = updateDto.TimeSpent;
            mockExam.UpdatedAt = DateTime.UtcNow;

            // Update answers if provided
            if (updateDto.Questions != null && updateDto.Questions.Any())
            {
                foreach (var questionDto in updateDto.Questions)
                {
                    var answer = mockExam.MockExamAnswers
                        .FirstOrDefault(a => a.QuestionId == questionDto.QuestionId);

                    if (answer != null)
                    {
                        answer.SelectedOptionId = questionDto.SelectedOptionId;
                        // Check if selected answer is correct
                        if (questionDto.SelectedOptionId.HasValue)
                        {
                            var correctOption = await _unitOfWork.AnswerOptions.GetOneAsync(
                                filter: o => o.QuestionId == questionDto.QuestionId && o.IsCorrect
                            );
                            answer.IsCorrect = correctOption != null &&
                                             correctOption.OptionId == questionDto.SelectedOptionId;
                        }
                    }
                }
            }

            await _unitOfWork.MockExams.UpdateAsync(mockExam);
            await _unitOfWork.SaveChangesAsync();

            return await GetMockExamAsync(examId);
        }
        catch (Exception ex)
        {
            return ApiResponse<MockExamDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Private method: Update user statistics after exam
    /// </summary>
    /// 
    public async Task<ApiResponse<IEnumerable<MockExamDto>>> GetMockExamAsyncByUserIdAnfLicenseType(string userId, int licenseTypeId)
    {
        try
        {
            var mockExam = await _unitOfWork.MockExams.GetListAsync(
                filter: m => m.UserId == userId && m.LicenseType.LicenseTypeId == licenseTypeId);
            if (mockExam.Count() == 0)
                return ApiResponse<IEnumerable<MockExamDto>>.ErrorResponse("Mock exam not found");
            List<MockExamDto> examDto = mockExam.Select(me => new MockExamDto
            {
                ExamId = me.ExamId,
                UserId = me.UserId,
                LicenseTypeId = me.LicenseTypeId,
                LicenseTypeName = me.LicenseType?.LicenseName ?? string.Empty,
                TotalQuestions = me.TotalQuestions,
                CorrectAnswers = me.CorrectAnswers,
                WrongAnswers = me.WrongAnswers,
                PassingScore = me.PassingScore,
                PassStatus = me.PassStatus,
                IsSubmitted = me.IsSubmitted,
                TimeSpent = me.TimeSpent ?? 0,
                StartedAt = me.StartedAt,
                CompletedAt = me.CompletedAt,
                CreatedAt = me.CreatedAt
            }).ToList();
            return ApiResponse<IEnumerable<MockExamDto>>.SuccessResponse(examDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<MockExamDto>>.ErrorResponse(ex.Message);
        }
    }
    private async Task UpdateUserStatisticsAsync(string userId, bool isPassed)
    {
        var mockExam = await _unitOfWork.MockExams.GetListAsync(
            filter: m => m.UserId == userId,
            include: m => m.Include(x => x.LicenseType)
        );
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
            foreach (var exam in mockExam)
            {
                statistic.HighestScore = Math.Max(statistic.HighestScore, exam.Score);
                statistic.LowestScore = statistic.LowestScore == 0 ? exam.Score : Math.Min(statistic.LowestScore, exam.Score);
                statistic.TotalQuestionsAnswered += exam.TotalQuestions;

                statistic.TotalCorrectAnswers += exam.CorrectAnswers;
                statistic.TotalLearningTime += exam.TimeSpent ?? 0;
                statistic.AverageScore = statistic.TotalExamsTaken == 0 ? 0 :
                    ((statistic.AverageScore * (statistic.TotalExamsTaken - 1)) + exam.Score) / statistic.TotalExamsTaken;

                statistic.AccuracyRate = statistic.TotalQuestionsAnswered == 0 ? 0 :
                    (decimal)statistic.TotalCorrectAnswers / statistic.TotalQuestionsAnswered * 100;
            }
            statistic.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.UserStatistics.UpdateAsync(statistic);
        }
        else
        {
            // Logic mới: Tạo mới nếu chưa có
            var newStatistic = new UserStatistic
            {

                UserId = userId,
                TotalExamsTaken = 1,
                TotalExamsPassed = isPassed ? 1 : 0,
                TotalExamsFailed = isPassed ? 0 : 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserStatistics.AddAsync(newStatistic);
        }
    }
}