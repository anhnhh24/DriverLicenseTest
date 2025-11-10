using AutoMapper;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.Question;
using Microsoft.EntityFrameworkCore;

namespace DriverLicenseTest.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuestionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<QuestionDto>>> GetRandomEliminationQuestionsAsync(int totalQuestions)
    {
        try
        {
            // Validate
            totalQuestions = Math.Max(1, Math.Min(totalQuestions, 100)); // Max 100

            // Get all elimination questions with answer options (fixed: call repository method with filter/include,
            // do not await then try to compose EF query on the result)
            var eliminationQuestionsEnumerable = await _unitOfWork.Questions.GetListAsync(
                filter: q => q.IsElimination == true,
                include: q => q.Include(x => x.AnswerOptions).Include(x => x.Category)
            );

            var eliminationQuestions = eliminationQuestionsEnumerable.ToList();

            if (!eliminationQuestions.Any())
                return ApiResponse<List<QuestionDto>>.ErrorResponse("No elimination questions available");

            if (eliminationQuestions.Count < totalQuestions)
            {
                return ApiResponse<List<QuestionDto>>.ErrorResponse(
                    $"Only {eliminationQuestions.Count} elimination questions available, requested {totalQuestions}");
            }

            // Get random questions
            var randomQuestions = eliminationQuestions
                .OrderBy(x => Guid.NewGuid())
                .Take(totalQuestions)
                .ToList();

            var questionDtos = _mapper.Map<List<QuestionDto>>(randomQuestions);

            return ApiResponse<List<QuestionDto>>.SuccessResponse(questionDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<QuestionDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get random questions by category for mock exam
    /// 
    /// Example: GetRandomQuestionsByCategoryAsync(1, 40)
    /// -> Trả về 40 câu hỏi ngẫu nhiên từ category 1 (Biển báo)
    /// </summary>
    public async Task<ApiResponse<List<QuestionDto>>> GetRandomQuestionsByCategoryAsync(
        int categoryId, int totalQuestions)
    {
        try
        {
            // Validate
            if (categoryId <= 0)
                return ApiResponse<List<QuestionDto>>.ErrorResponse("Category ID must be greater than 0");

            totalQuestions = Math.Max(1, Math.Min(totalQuestions, 100)); // Max 100

            // Check if category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
                return ApiResponse<List<QuestionDto>>.ErrorResponse("Category not found");

         
            var categoryQuestionsEnumerable = await _unitOfWork.Questions.GetListAsync(
                filter: q => q.CategoryId == categoryId,
                orderBy: q => q.OrderBy(x => x.QuestionNumber),
                include: q => q.Include(x => x.AnswerOptions).Include(x => x.Category)
            );

            var categoryQuestions = categoryQuestionsEnumerable.ToList();

            if (!categoryQuestions.Any())
                return ApiResponse<List<QuestionDto>>.ErrorResponse(
                    $"No questions available in category: {category.CategoryName}");

            if (categoryQuestions.Count < totalQuestions)
            {
                return ApiResponse<List<QuestionDto>>.ErrorResponse(
                    $"Only {categoryQuestions.Count} questions available in {category.CategoryName}, requested {totalQuestions}");
            }

            // Get random questions
            var randomQuestions = categoryQuestions
                .OrderBy(x => Guid.NewGuid())
                .Take(totalQuestions)
                .ToList();

            var questionDtos = _mapper.Map<List<QuestionDto>>(randomQuestions);

            return ApiResponse<List<QuestionDto>>.SuccessResponse(questionDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<QuestionDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get question by ID with answer options
    /// </summary>
    public async Task<ApiResponse<QuestionDto>> GetQuestionByIdAsync(int id)
    {
        try
        {
            var question = await _unitOfWork.Questions.GetOneAsync(
                filter: q => q.QuestionId == id,
                include: q => q.Include(x => x.AnswerOptions).Include(x => x.Category)
            );

            if (question == null)
                return ApiResponse<QuestionDto>.ErrorResponse("Question not found");

            var questionDto = _mapper.Map<QuestionDto>(question);
            return ApiResponse<QuestionDto>.SuccessResponse(questionDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<QuestionDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get question by question number
    /// </summary>
    public async Task<ApiResponse<QuestionDto>> GetQuestionByNumberAsync(int questionNumber)
    {
        try
        {
            var question = await _unitOfWork.Questions.GetOneAsync(
                filter: q => q.QuestionNumber == questionNumber,
                include: q => q.Include(x => x.AnswerOptions).Include(x => x.Category)
            );

            if (question == null)
                return ApiResponse<QuestionDto>.ErrorResponse("Question not found");

            var questionDto = _mapper.Map<QuestionDto>(question);
            return ApiResponse<QuestionDto>.SuccessResponse(questionDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<QuestionDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all questions with pagination - OPTIMIZED
    /// </summary>
    public async Task<ApiResponse<PagedResult<QuestionDto>>> GetQuestionsAsync(int pageNumber, int pageSize)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
    pageSize = Math.Max(1, Math.Min(pageSize, 100)); // Max 100 per page

   // Get total count first - NO INCLUDE needed for count
var totalCount = await _unitOfWork.Questions.GetCount();

            // Get paginated questions - ONLY include Category (no AnswerOptions for list view)
   var questions = await _unitOfWork.Questions.GetListAsync(
           orderBy: q => q.OrderBy(x => x.QuestionNumber),
    include: q => q.Include(c => c.Category), // Only Category needed for list
  pageSize: pageSize,
         pageNumber: pageNumber
     );

     var questionDtos = _mapper.Map<List<QuestionDto>>(questions);

       var pagedResult = new PagedResult<QuestionDto>
      {
                Items = questionDtos,
                TotalCount = totalCount,
    PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ApiResponse<PagedResult<QuestionDto>>.SuccessResponse(pagedResult);
   }
        catch (Exception ex)
 {
            return ApiResponse<PagedResult<QuestionDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get questions by category with pagination - OPTIMIZED
    /// </summary>
    public async Task<ApiResponse<PagedResult<QuestionDto>>> GetQuestionsByCategoryAsync(
        int categoryId, int pageNumber, int pageSize)
    {
        try
     {
   pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

      // Get total count for this category first
    var totalCount = await _unitOfWork.Questions.GetCount(q => q.CategoryId == categoryId);

            // Get paginated questions - ONLY include Category
      var questions = await _unitOfWork.Questions.GetListAsync(
       filter: q => q.CategoryId == categoryId,
       orderBy: q => q.OrderBy(x => x.QuestionNumber),
     include: q => q.Include(c => c.Category), // Only Category needed
        pageSize: pageSize,
           pageNumber: pageNumber
     );

 var questionDtos = _mapper.Map<List<QuestionDto>>(questions);

       var pagedResult = new PagedResult<QuestionDto>
          {
         Items = questionDtos,
   TotalCount = totalCount,
    PageNumber = pageNumber,
            PageSize = pageSize,
  TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

         return ApiResponse<PagedResult<QuestionDto>>.SuccessResponse(pagedResult);
      }
      catch (Exception ex)
        {
      return ApiResponse<PagedResult<QuestionDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get elimination questions (điểm liệt) - OPTIMIZED
    /// </summary>
    public async Task<ApiResponse<PagedResult<QuestionDto>>> GetEliminationQuestionsAsync(
  int pageNumber = 1, int pageSize = 20)
    {
 try
        {
            pageNumber = Math.Max(1, pageNumber);
      pageSize = Math.Max(1, Math.Min(pageSize, 100));

      // Get total count for elimination questions first
    var totalCount = await _unitOfWork.Questions.GetCount(q => q.IsElimination == true);

    // Get paginated questions - NO AnswerOptions for list view
         var questions = await _unitOfWork.Questions.GetListAsync(
   filter: q => q.IsElimination == true,
              orderBy: q => q.OrderBy(x => x.QuestionNumber),
                include: q => q.Include(c => c.Category), // Add Category for display
     pageSize: pageSize,
                pageNumber: pageNumber
            );

            var questionDtos = _mapper.Map<List<QuestionDto>>(questions);

  var pagedResult = new PagedResult<QuestionDto>
     {
           Items = questionDtos,
   TotalCount = totalCount,
     PageNumber = pageNumber,
           PageSize = pageSize,
    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
    };

return ApiResponse<PagedResult<QuestionDto>>.SuccessResponse(pagedResult);
        }
      catch (Exception ex)
        {
            return ApiResponse<PagedResult<QuestionDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Search questions by keyword - OPTIMIZED
/// </summary>
    public async Task<ApiResponse<List<QuestionDto>>> SearchQuestionsAsync(string keyword)
    {
        try
    {
            if (string.IsNullOrWhiteSpace(keyword))
     return ApiResponse<List<QuestionDto>>.ErrorResponse("Keyword cannot be empty");

            // Only include Category, no AnswerOptions for search results
            var questions = await _unitOfWork.Questions.GetListAsync(
  filter: q => q.QuestionText.Contains(keyword) ||
      (q.ExplanationText != null && q.ExplanationText.Contains(keyword)),
    orderBy: q => q.OrderBy(x => x.QuestionNumber),
include: q => q.Include(x => x.Category) // Only Category needed
            );

          var questionDtos = _mapper.Map<List<QuestionDto>>(questions);
            return ApiResponse<List<QuestionDto>>.SuccessResponse(questionDtos);
        }
        catch (Exception ex)
        {
  return ApiResponse<List<QuestionDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new question (Admin)
    /// </summary>
    public async Task<ApiResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto dto)
    {
        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.QuestionText))
                return ApiResponse<QuestionDto>.ErrorResponse("Question text is required");

            if (dto.AnswerOptions == null || !dto.AnswerOptions.Any())
                return ApiResponse<QuestionDto>.ErrorResponse("At least one answer option is required");

            // Create question
            var question = new Question
            {
                CategoryId = dto.CategoryId,
                QuestionText = dto.QuestionText,
                ExplanationText = dto.ExplanationText,
                DifficultyLevel = dto.DifficultyLevel ?? "Medium",
                IsElimination = dto.IsElimination,
                TimeLimit = dto.TimeLimit,
                Points = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Questions.AddAsync(question);
            await _unitOfWork.SaveChangesAsync();

            var questionDto = _mapper.Map<QuestionDto>(question);
            return ApiResponse<QuestionDto>.SuccessResponse(questionDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<QuestionDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing question (Admin)
    /// </summary>
    public async Task<ApiResponse<QuestionDto>> UpdateQuestionAsync(int id, CreateQuestionDto dto)
    {
        try
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(id);
            if (question == null)
                return ApiResponse<QuestionDto>.ErrorResponse("Question not found");

            // Update fields
            question.QuestionText = dto.QuestionText;
            question.ExplanationText = dto.ExplanationText;
            question.CategoryId = dto.CategoryId;
            question.DifficultyLevel = dto.DifficultyLevel ?? "Medium";
            question.IsElimination = dto.IsElimination;
            question.TimeLimit = dto.TimeLimit;
            question.Points = 1;
            question.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Questions.UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            var questionDto = _mapper.Map<QuestionDto>(question);
            return ApiResponse<QuestionDto>.SuccessResponse(questionDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<QuestionDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a question (Admin)
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteQuestionAsync(int id)
    {
        try
        {
            var question = await _unitOfWork.Questions.GetByIdAsync(id);
            await _unitOfWork.Questions.DeleteAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
        }
    }
}