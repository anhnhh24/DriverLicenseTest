using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Shared.DTOs.Question;

namespace AdminApp.Pages.Questions;

public class IndexModel : PageModel
{
    private readonly IQuestionService _questionService;

    public IndexModel(IQuestionService questionService)
    {
        _questionService = questionService;
  }

    public List<QuestionDto> Questions { get; set; } = new();

  [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

  [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
     var result = await _questionService.GetQuestionsAsync(PageNumber, PageSize);
      
        if (result.Success && result.Data != null)
        {
     Questions = result.Data.Items;
            TotalCount = result.Data.TotalCount;
      TotalPages = result.Data.TotalPages;
 PageNumber = result.Data.PageNumber;
}
    }
}