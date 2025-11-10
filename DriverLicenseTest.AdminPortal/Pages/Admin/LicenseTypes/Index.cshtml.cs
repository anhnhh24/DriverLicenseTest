// LicenseTypes/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;

namespace AdminApp.Pages.Admin.LicenseTypes
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<LicenseType> LicenseTypes { get; set; } = new();

        public async Task OnGetAsync()
        {
            LicenseTypes = (await _unitOfWork.LicenseTypes.GetListAsync(
                orderBy: q => q.OrderBy(l => l.LicenseCode)
            )).ToList();
        }
    }
}
