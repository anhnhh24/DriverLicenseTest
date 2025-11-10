// Users/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using System.Linq.Expressions;

namespace AdminApp.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
            
        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<AspNetUser> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            Expression<Func<AspNetUser, bool>>? filter = null;

            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = u => (u.Email != null && u.Email.Contains(SearchString)) ||
                             (u.FullName != null && u.FullName.Contains(SearchString)) ||
                             (u.UserName != null && u.UserName.Contains(SearchString));
            }

            TotalCount = await _unitOfWork.Users.GetCount(filter);
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Users = (List<AspNetUser>)await _unitOfWork.Users.GetListAsync(
                filter: filter,
                orderBy: q => q.OrderByDescending(u => u.CreatedAt),
                pageSize: PageSize,
                pageNumber: PageNumber
            );
        }
    }
}
