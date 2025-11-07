using Microsoft.AspNetCore.Mvc;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Shared.DTOs.Category;
using AutoMapper;

namespace DriverLicenseTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _unitOfWork.Categories.GetListAsync();
        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
        categoryDtos[0].QuestionCount = 180;
        categoryDtos[1].QuestionCount = 25;
        categoryDtos[2].QuestionCount = 58;
        categoryDtos[3].QuestionCount = 37;
        categoryDtos[4].QuestionCount = 185;
        categoryDtos[5].QuestionCount = 115;

        return Ok(new
        {
            success = true,
            data = categoryDtos.OrderBy(c => c.OrderIndex)
        });
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null)
            return NotFound(new { success = false, message = "Category not found" });

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(new { success = true, data = categoryDto });
    }
}
