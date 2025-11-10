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
    public async Task<ApiResponse<MockExamDto>> SubmitMockExam([FromRoute] int examId, [FromBody] SubmitExamRequest request)
    {
        if (examId != request.ExamId)
        {
            return new ApiResponse<MockExamDto>
            {
                Data = null,
                Success = false,
                Message = "Mã bài thi không khớp giữa đường dẫn và nội dung yêu cầu."
            };
        }
        try
        {
            var resultExam = await _mockExamService.SubmitMockExamAsync(examId ,request);

            return resultExam;
        }
        catch (Exception ex)
        {
            return new ApiResponse<MockExamDto>
            {
                Data = null,
                Success = false,
                Message = "Đã xảy ra lỗi khi nộp bài thi: " + ex.Message
            };
    }
}

    [HttpGet("byuser")]
    public async Task<IActionResult> GetMockExamByUserIdAndLicenseType([FromQuery] string userId, [FromQuery] int licenseType)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(ApiResponse<MockExamDto>.ErrorResponse("userId and licenseType are required"));
        var result = await _mockExamService.GetMockExamAsyncByUserIdAnfLicenseType(userId, licenseType);
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpPut("update/{examId}")]
    public async Task<IActionResult> UpdateMockExam(int examId, [FromQuery] string userId, [FromBody] MockExamDto updateDto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(ApiResponse<MockExamDto>.ErrorResponse("userId is required"));
        var result = await _mockExamService.UpdateMockExamAsync(examId, userId, updateDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}