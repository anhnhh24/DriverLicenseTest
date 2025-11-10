// Users/Details.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminApp.Pages.Admin.Users
{
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public AspNetUser User { get; set; }
        public UserStatistic? Statistics { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User = await _unitOfWork.Users.GetByIdAsync(id);

            if (User == null)
            {
                return NotFound();
            }

            // Load statistics
            Statistics = await _unitOfWork.UserStatistics.GetOneAsync(
                filter: s => s.UserId == id
            );

            return Page();
        }
    }
}
