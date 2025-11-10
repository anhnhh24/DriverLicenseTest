using DriverLicenseTest.Shared.DTOs.Common;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserStatisticsController : ControllerBase
{
    private readonly IUserStatisticService _statisticService;

    public UserStatisticsController(IUserStatisticService statisticService)
    {
        _statisticService = statisticService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<UserStatisticDto>>> GetUserStatistic(string userId)
    {
        var response = await _statisticService.GetUserStatisticAsync(userId!);
        return Ok(response);
    }

    [HttpPost("update/{userId}")]
    public async Task<ActionResult<ApiResponse<UserStatisticDto>>> UpdateUserStatistic(string userId)
    {
        var response = await _statisticService.UpdateUserStatisticAsync(userId!);
        return Ok(response);
    }
}