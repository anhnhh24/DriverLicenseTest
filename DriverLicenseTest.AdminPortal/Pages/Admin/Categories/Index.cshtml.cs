using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdminApp.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<CategoryDto> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            // Build filter
            Expression<Func<Category, bool>>? filter = null;
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = c => c.CategoryName.Contains(SearchString);
            }

            // Get total count
            TotalCount = await _unitOfWork.Categories.GetCount(filter);
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Get categories
            var categories = await _unitOfWork.Categories.GetListAsync(
                filter: filter,
                orderBy: q => q.OrderBy(c => c.OrderIndex),
                include: q => q.Include(c => c.Questions),
                pageSize: PageSize,
                pageNumber: PageNumber
            );

            Categories = categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                OrderIndex = c.OrderIndex,
                QuestionCount = c.Questions.Count
            }).ToList();
        }
    }
}
