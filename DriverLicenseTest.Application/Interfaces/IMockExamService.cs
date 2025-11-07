using DriverLicenseTest.Shared.DTOs.Common;
using DriverLicenseTest.Shared.DTOs.MockExam;

namespace DriverLicenseTest.Application.Services.Interfaces;

public interface IMockExamService
{
    Task<ApiResponse<MockExamDto>> StartMockExamAsync(string userId, string licenseType);

    Task<ApiResponse<MockExamDto>> GetMockExamAsync(int examId);
    Task<ApiResponse<MockExamDto>> SubmitMockExamAsync(int examId);
}
