using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.TrafficSign;

namespace DriverLicenseTest.Application.Services;

public class TrafficSignService : ITrafficSignService
{
    private readonly IUnitOfWork _unitOfWork;

    public TrafficSignService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<TrafficSignDto>>> GetTrafficSignsAsync(
        int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            // OPTIMIZED: Get total count first
            var totalCount = await _unitOfWork.TrafficSigns.GetCount(s => s.IsActive);

            // OPTIMIZED: Get paginated data directly from database
            var signs = await _unitOfWork.TrafficSigns.GetListAsync(
                filter: s => s.IsActive,
                orderBy: q => q.OrderBy(s => s.SignCode),
                pageSize: pageSize,
                pageNumber: pageNumber
             );

            // OPTIMIZED: Map directly without ToList() first
            var signDtos = signs.Select(s => new TrafficSignDto
                {
                    SignId = s.SignId,
                    SignCode = s.SignCode,
                    SignName = s.SignName,
                    Description = s.Description,
                    ImageURL = s.ImageUrl,
                    SignType = s.SignType,
                    Meaning = s.Meaning,
                    RelatedQuestionCount = s.RelatedQuestionCount,
                    IsActive = s.IsActive
                }).ToList();

            var pagedResult = new PagedResult<TrafficSignDto>
            {
                Items = signDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return ApiResponse<PagedResult<TrafficSignDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<TrafficSignDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrafficSignDto>> GetTrafficSignByIdAsync(int signId)
    {
        try
        {
            var sign = await _unitOfWork.TrafficSigns.GetByIdAsync(signId);

            if (sign == null)
                return ApiResponse<TrafficSignDto>.ErrorResponse("Traffic sign not found");

            var signDto = new TrafficSignDto
            {
                SignId = sign.SignId,
                SignCode = sign.SignCode,
                SignName = sign.SignName,
                Description = sign.Description,
                ImageURL = sign.ImageUrl,
                SignType = sign.SignType,
                Meaning = sign.Meaning,
                RelatedQuestionCount = sign.RelatedQuestionCount,
                IsActive = sign.IsActive
            };

            return ApiResponse<TrafficSignDto>.SuccessResponse(signDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<TrafficSignDto>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TrafficSignDto>>> GetTrafficSignsByTypeAsync(string signType)
    {
        try
        {
            // OPTIMIZED: Filter at database level
            var signs = await _unitOfWork.TrafficSigns.GetListAsync(
                filter: s => s.SignType == signType && s.IsActive,
                orderBy: q => q.OrderBy(s => s.SignCode)
             );

            // OPTIMIZED: Map directly
            var signDtos = signs.Select(s => new TrafficSignDto
                {
                    SignId = s.SignId,
                    SignCode = s.SignCode,
                    SignName = s.SignName,
                    Description = s.Description,
                    ImageURL = s.ImageUrl,
                    SignType = s.SignType,
                    Meaning = s.Meaning,
                    RelatedQuestionCount = s.RelatedQuestionCount,
                    IsActive = s.IsActive
                }).ToList();

            return ApiResponse<List<TrafficSignDto>>.SuccessResponse(signDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TrafficSignDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TrafficSignDto>>> SearchTrafficSignsAsync(string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return ApiResponse<List<TrafficSignDto>>.ErrorResponse("Keyword is required");

            // OPTIMIZED: Filter at database level
            var signs = await _unitOfWork.TrafficSigns.GetListAsync(
                filter: s => (s.SignName.Contains(keyword) || s.SignCode.Contains(keyword)) && s.IsActive,
                orderBy: q => q.OrderBy(s => s.SignCode)
             );

            // OPTIMIZED: Map directly
            var signDtos = signs.Select(s => new TrafficSignDto
                {
                    SignId = s.SignId,
                    SignCode = s.SignCode,
                    SignName = s.SignName,
                    Description = s.Description,
                    ImageURL = s.ImageUrl,
                    SignType = s.SignType,
                    Meaning = s.Meaning,
                    RelatedQuestionCount = s.RelatedQuestionCount,
                    IsActive = s.IsActive
                }).ToList();

            return ApiResponse<List<TrafficSignDto>>.SuccessResponse(signDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TrafficSignDto>>.ErrorResponse($"Error: {ex.Message}");
        }
    }
}
