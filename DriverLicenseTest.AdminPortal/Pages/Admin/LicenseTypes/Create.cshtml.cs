// LicenseTypes/Create.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AdminApp.Pages.Admin.LicenseTypes
{
    public class CreateModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public LicenseTypeInputModel License { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var licenseType = new LicenseType
            {
                LicenseCode = License.LicenseCode,
                LicenseName = License.LicenseName,
                VehicleType = License.VehicleType,
                TotalQuestions = License.TotalQuestions,
                TimeLimit = License.TimeLimit,
                PassingScore = License.PassingScore,
                RequiredElimination = License.RequiredElimination,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.LicenseTypes.AddAsync(licenseType);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm loại bằng lái mới thành công!";
            return RedirectToPage("Index");
        }
    }

    public class LicenseTypeInputModel
    {
        [Required(ErrorMessage = "Mã bằng là bắt buộc")]
        [StringLength(10)]
        public string LicenseCode { get; set; }

        [Required(ErrorMessage = "Tên bằng lái là bắt buộc")]
        [StringLength(200)]
        public string LicenseName { get; set; }

        [StringLength(100)]
        public string? VehicleType { get; set; }

        [Required(ErrorMessage = "Tổng số câu hỏi là bắt buộc")]
        [Range(1, 200)]
        public int TotalQuestions { get; set; } = 30;

        [Required(ErrorMessage = "Thời gian làm bài là bắt buộc")]
        [Range(1, 180)]
        public int TimeLimit { get; set; } = 30;

        [Required(ErrorMessage = "Điểm đạt là bắt buộc")]
        [Range(1, 100)]
        public int PassingScore { get; set; } = 21;

        [Range(0, 10)]
        public int? RequiredElimination { get; set; } = 0;
    }
}
