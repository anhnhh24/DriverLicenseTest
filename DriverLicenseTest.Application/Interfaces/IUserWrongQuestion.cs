using DriverLicenseTest.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverLicenseTest.Application.Interfaces
{
    public interface IUserWrongQuestionService
    {
        Task<ApiResponse<IEnumerable<UserWrongQuestionDto>>> GetUserWrongQuestionsAsync(string userId);
        Task<ApiResponse<IEnumerable<UserWrongQuestionDto>>> GetUserWrongQuestionsByLicenseAsync(int licenseTypeId, string userId);
        Task<ApiResponse<bool>> MarkQuestionAsFixedAsync(int wrongQuestionId, string userId);
    }
}
