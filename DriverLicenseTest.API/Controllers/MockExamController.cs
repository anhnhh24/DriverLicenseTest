using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.MockExam;
using Microsoft.AspNetCore.Mvc;

namespace DriverLicenseTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MockExamsController : ControllerBase
{
    private readonly IMockExamService _mockExamService;

    public MockExamsController(IMockExamService mockExamService)
    {
        _mockExamService = mockExamService;
    }

    /// <summary>
    /// Start a new mock exam for a user and license type.
    /// POST api/mockexams/start?userId=...&licenseType=...
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartMockExam([FromQuery] string userId, [FromQuery] string licenseType)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(licenseType))
            return BadRequest(ApiResponse<MockExamDto>.ErrorResponse("userId and licenseType are required"));

        var result = await _mockExamService.StartMockExamAsync(userId, licenseType);
        if (!result.Success)
            return BadRequest(result);

        // return 201 with location to GET exam
        return CreatedAtAction(nameof(GetMockExam), new { examId = result.Data?.ExamId }, result);
    }

    /// <summary>
    /// Get mock exam details
    /// GET api/mockexams/{examId}
    /// </summary>
    [HttpGet("{examId:int}")]
    public async Task<IActionResult> GetMockExam(int examId)
    {
        var result = await _mockExamService.GetMockExamAsync(examId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Submit mock exam and calculate results
    /// POST api/mockexams/{examId}/submit
    /// </summary>
    [HttpPost("{examId:int}/submit")]
    public async Task<IActionResult> SubmitMockExam(int examId)
    {
        var result = await _mockExamService.SubmitMockExamAsync(examId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}