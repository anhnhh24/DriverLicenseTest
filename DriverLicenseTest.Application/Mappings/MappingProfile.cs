using AutoMapper;
using DriverLicenseTest.Domain.Entities;
using DriverLicenseTest.Shared.DTOs.Category;
using DriverLicenseTest.Shared.DTOs.Question;

namespace DriverLicenseTest.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions.OrderBy(a => a.OptionOrder)));
        CreateMap<AnswerOption, AnswerOptionDto>();

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count));
    }
}
