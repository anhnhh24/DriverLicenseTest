using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Shared.DTOs.Common;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserWrongQuestionsController : ControllerBase
{
    private readonly IUserWrongQuestionService _wrongQuestionService;

    public UserWrongQuestionsController(IUserWrongQuestionService wrongQuestionService)
    {
        _wrongQuestionService = wrongQuestionService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserWrongQuestionDto>>>> GetUserWrongQuestions(string userId)
    {
        var response = await _wrongQuestionService.GetUserWrongQuestionsAsync(userId);
        return Ok(response);
    }

    [HttpGet("category/{licenseTypeId}/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserWrongQuestionDto>>>> GetUserWrongQuestionsByLicense(
        int licenseTypeId, string userId)
    {
        var response = await _wrongQuestionService.GetUserWrongQuestionsByLicenseAsync(licenseTypeId, userId);
        return Ok(response);
    }

    [HttpPut("{wrongQuestionId}/mark-fixed")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkQuestionAsFixed(int wrongQuestionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _wrongQuestionService.MarkQuestionAsFixedAsync(wrongQuestionId, userId);
        return Ok(response);
    }
}