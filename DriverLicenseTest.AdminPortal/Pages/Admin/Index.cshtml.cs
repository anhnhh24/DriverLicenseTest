using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Question;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdminApp.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public int TotalQuestions { get; set; }
public int TotalTrafficSigns { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
  public List<QuestionDto> RecentQuestions { get; set; } = new();
   public List<LicenseType> LicenseTypes { get; set; } = new();

        public IndexModel(IUnitOfWork unitOfWork)
        {
    _unitOfWork = unitOfWork;
        }

 public async Task OnGetAsync()
   {
 // Execute count queries sequentially (DbContext is not thread-safe)
            TotalQuestions = await _unitOfWork.Questions.GetCount();
            TotalTrafficSigns = await _unitOfWork.TrafficSigns.GetCount();
            TotalUsers = await _unitOfWork.Users.GetCount();
 TotalCategories = await _unitOfWork.Categories.GetCount();

// OPTIMIZED: Only include Category (no AnswerOptions needed), get only 5 records
       var recentQuestionsData = await _unitOfWork.Questions.GetListAsync(
          orderBy: q => q.OrderByDescending(x => x.CreatedAt),
        include: q => q.Include(x => x.Category),
       pageSize: 5,
       pageNumber: 1
      );

     // OPTIMIZED: Map only needed fields
            RecentQuestions = recentQuestionsData.Select(q => new QuestionDto
{
                QuestionId = q.QuestionId,
       QuestionNumber = q.QuestionNumber,
        QuestionText = q.QuestionText,
          CategoryName = q.Category?.CategoryName ?? "N/A"
}).ToList();

       // OPTIMIZED: Get only LicenseTypes (no related collections)
          LicenseTypes = (await _unitOfWork.LicenseTypes.GetListAsync(
            orderBy: q => q.OrderBy(l => l.LicenseCode)
          )).ToList();
        }
    }
}
