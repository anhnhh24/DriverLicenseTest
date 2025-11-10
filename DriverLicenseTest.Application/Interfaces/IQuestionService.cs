using DriverLicenseTest.Shared.DTOs;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.Question;

namespace DriverLicenseTest.Application.Services.Interfaces;

public interface IQuestionService
{
    Task<ApiResponse<QuestionDto>> GetQuestionByIdAsync(int id);
    Task<ApiResponse<QuestionDto>> GetQuestionByNumberAsync(int questionNumber);
    Task<ApiResponse<PagedResult<QuestionDto>>> GetQuestionsAsync(int pageNumber, int pageSize);
    Task<ApiResponse<PagedResult<QuestionDto>>> GetQuestionsByCategoryAsync(int categoryId, int pageNumber, int pageSize);
    Task<ApiResponse<QuestionDto>> CreateQuestionAsync(CreateQuestionDto dto);
    Task<ApiResponse<QuestionDto>> UpdateQuestionAsync(int id, CreateQuestionDto dto);
    Task<ApiResponse<bool>> DeleteQuestionAsync(int id);
    Task<ApiResponse<List<QuestionDto>>> SearchQuestionsAsync(string keyword);
    Task<ApiResponse<List<QuestionDto>>> GetRandomEliminationQuestionsAsync(int totalQuestions);
    Task<ApiResponse<List<QuestionDto>>> GetRandomQuestionsByCategoryAsync(int categoryId, int totalQuestions);
}
