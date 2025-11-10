using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Shared.DTOs;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.Question;
using Microsoft.AspNetCore.Mvc;


namespace DriverLicenseTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet("random/elimination")]
    public async Task<IActionResult> GetRandomEliminationQuestions([FromQuery] int count = 10)
    {
        var result = await _questionService.GetRandomEliminationQuestionsAsync(count);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("random/category/{categoryId}")]
    public async Task<IActionResult> GetRandomQuestionsByCategory(int categoryId, [FromQuery] int count = 10)
    {
        if (categoryId <= 0) return BadRequest(ApiResponse<List<QuestionDto>>.ErrorResponse("CategoryId must be > 0"));
        if (count <= 0) return BadRequest(ApiResponse<List<QuestionDto>>.ErrorResponse("Count must be > 0"));

        var result = await _questionService.GetRandomQuestionsByCategoryAsync(categoryId, count);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuestionById(int id)
    {
        var result = await _questionService.GetQuestionByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpGet("number/{questionNumber}")]
    public async Task<IActionResult> GetQuestionByNumber(int questionNumber)
    {
        var result = await _questionService.GetQuestionByNumberAsync(questionNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetQuestions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _questionService.GetQuestionsAsync(pageNumber, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetQuestionsByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _questionService.GetQuestionsByCategoryAsync(categoryId, pageNumber, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchQuestions([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(ApiResponse<List<QuestionDto>>.ErrorResponse("Keyword is required"));

        var result = await _questionService.SearchQuestionsAsync(keyword);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _questionService.CreateQuestionAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetQuestionById), new { id = result.Data?.QuestionId }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] CreateQuestionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _questionService.UpdateQuestionAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

}