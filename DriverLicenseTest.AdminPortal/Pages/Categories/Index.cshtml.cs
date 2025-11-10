using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Shared.DTOs.Category;
using System.Linq.Expressions;
using DriverLicenseTest.Domain.Entities;

namespace AdminApp.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
 _unitOfWork = unitOfWork;
    }

    public List<CategoryDto> Categories { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
     Expression<Func<Category, bool>>? filter = null;

        if (!string.IsNullOrEmpty(SearchString))
    {
   filter = c => c.CategoryName.Contains(SearchString) ||
   (c.Description != null && c.Description.Contains(SearchString));
   }

        TotalCount = await _unitOfWork.Categories.GetCount(filter);
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

        var categories = await _unitOfWork.Categories.GetListAsync(
        filter: filter,
     orderBy: q => q.OrderBy(c => c.OrderIndex),
       pageSize: PageSize,
       pageNumber: PageNumber
      );

       
        Categories = new List<CategoryDto>();
    foreach (var category in categories)
    {
            var questionCount = await _unitOfWork.Questions.GetCount(q => q.CategoryId == category.CategoryId);
            
            Categories.Add(new CategoryDto
            {
             CategoryId = category.CategoryId,
    CategoryName = category.CategoryName,
    Description = category.Description,
     OrderIndex = category.OrderIndex,
                QuestionCount = questionCount
      });
        }
    }
}