using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.TrafficSign;
using Microsoft.AspNetCore.Mvc;

namespace DriverLicenseTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrafficSignsController : ControllerBase
{
    private readonly ITrafficSignService _trafficSignService;

    public TrafficSignsController(ITrafficSignService trafficSignService)
    {
        _trafficSignService = trafficSignService;
    }

    /// <summary>
    /// Get all traffic signs with pagination
    /// GET: /api/trafficsigns?pageNumber=1&pageSize=20
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTrafficSigns(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _trafficSignService.GetTrafficSignsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get traffic sign by ID
    /// GET: /api/trafficsigns/5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrafficSignById(int id)
    {
        var result = await _trafficSignService.GetTrafficSignByIdAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get traffic signs by type
    /// GET: /api/trafficsigns/type/Prohibition
    /// </summary>  
    [HttpGet("type/{signType}")]
    public async Task<IActionResult> GetTrafficSignsByType(string signType)
    {
        var result = await _trafficSignService.GetTrafficSignsByTypeAsync(signType);
        return Ok(result);
    }

    /// <summary>
    /// Search traffic signs by keyword
    /// GET: /api/trafficsigns/search?keyword=dừng
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchTrafficSigns([FromQuery] string keyword)
    {
        var result = await _trafficSignService.SearchTrafficSignsAsync(keyword);
        return Ok(result);
    }
}
