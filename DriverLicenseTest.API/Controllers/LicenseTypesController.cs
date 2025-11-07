using Microsoft.AspNetCore.Mvc;
using DriverLicenseTest.Application.Interfaces;

namespace DriverLicenseTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public LicenseTypesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all license types
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLicenseTypes()
    {
        var licenseTypes = await _unitOfWork.LicenseTypes.GetListAsync();

        return Ok(new
        {
            success = true,
            data = licenseTypes
        });
    }

    /// <summary>
    /// Get license type by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLicenseTypeById(int id)
    {
        var licenseType = await _unitOfWork.LicenseTypes.GetByIdAsync(id);

        if (licenseType == null)
            return NotFound(new { success = false, message = "License type not found" });

        return Ok(new { success = true, data = licenseType });
    }
}
