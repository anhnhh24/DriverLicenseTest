using AdminApp.Pages.Admin.TrafficSigns;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Category;
using DriverLicenseTest.Shared.DTOs.Question;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdminApp.Pages.Admin.Questions
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<QuestionDto> Questions { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DifficultyLevel { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            // Load categories for filter
            var categories = await _unitOfWork.Categories.GetListAsync(
                orderBy: q => q.OrderBy(c => c.OrderIndex)
            );
            Categories = categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            }).ToList();

            // Build filter
            Expression<Func<Question, bool>>? filter = null;

            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = q => q.QuestionText.Contains(SearchString);
            }

            if (CategoryId.HasValue)
            {
                var catFilter = new Func<Question, bool>(q => q.CategoryId == CategoryId.Value);
                filter = filter == null ?
                    (Expression<Func<Question, bool>>)(q => q.CategoryId == CategoryId.Value) :
                    filter.And(q => q.CategoryId == CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(DifficultyLevel))
            {
                var diffFilter = new Func<Question, bool>(q => q.DifficultyLevel == DifficultyLevel);
                filter = filter == null ?
                    (Expression<Func<Question, bool>>)(q => q.DifficultyLevel == DifficultyLevel) :
                    filter.And(q => q.DifficultyLevel == DifficultyLevel);
            }

            // Get total count
            TotalCount = await _unitOfWork.Questions.GetCount(filter);
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Get questions
            var questions = await _unitOfWork.Questions.GetListAsync(
                filter: filter,
                orderBy: q => q.OrderBy(x => x.QuestionNumber),
                include: q => q.Include(x => x.Category).Include(x => x.AnswerOptions),
                pageSize: PageSize,
                pageNumber: PageNumber
            );

            Questions = questions.Select(q => new QuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionNumber = q.QuestionNumber,
                QuestionText = q.QuestionText,
                CategoryName = q.Category.CategoryName,
                DifficultyLevel = q.DifficultyLevel ?? "Medium",
                IsElimination = q.IsElimination ?? false,
                ImageURL = q.ImageUrl,
                Points = q.Points ?? 1
            }).ToList();
        }

        public Dictionary<string, string> GetRouteData(int pageNumber)
        {
            var routeData = new Dictionary<string, string>
            {
                { "pageNumber", pageNumber.ToString() }
            };

            if (!string.IsNullOrEmpty(SearchString))
                routeData["searchString"] = SearchString;

            if (CategoryId.HasValue)
                routeData["categoryId"] = CategoryId.Value.ToString();

            if (!string.IsNullOrEmpty(DifficultyLevel))
                routeData["difficultyLevel"] = DifficultyLevel;

            return routeData;
        }
    }
}
