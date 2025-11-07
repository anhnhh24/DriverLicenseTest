using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.TrafficSign;

namespace DriverLicenseTest.Application.Services.Interfaces;

public interface ITrafficSignService
{
    /// <summary>
    /// Get all traffic signs with pagination
    /// </summary>
    Task<ApiResponse<PagedResult<TrafficSignDto>>> GetTrafficSignsAsync(
        int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get traffic sign by ID
    /// </summary>
    Task<ApiResponse<TrafficSignDto>> GetTrafficSignByIdAsync(int signId);

    /// <summary>
    /// Get traffic signs by type (e.g., "Prohibition", "Warning", "Mandatory")
    /// </summary>
    Task<ApiResponse<List<TrafficSignDto>>> GetTrafficSignsByTypeAsync(string signType);

    /// <summary>
    /// Search traffic signs by name or code
    /// </summary>
    Task<ApiResponse<List<TrafficSignDto>>> SearchTrafficSignsAsync(string keyword);
}
