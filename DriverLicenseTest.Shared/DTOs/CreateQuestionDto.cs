using System.ComponentModel.DataAnnotations;

namespace DriverLicenseTest.Shared.DTOs.Question;

public class CreateQuestionDto
{
    [Required]
    public int QuestionNumber { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    public string? ExplanationText { get; set; }

    [StringLength(20)]
    public string DifficultyLevel { get; set; } = "Medium";

    public bool IsElimination { get; set; }

    [Url]
    [StringLength(500)]
    public string? ImageURL { get; set; }

    [Range(10, 300)]
    public int TimeLimit { get; set; } = 30;

    [Range(1, 10)]
    public int Points { get; set; } = 1;

    [Required]
    [MinLength(2)]
    public List<CreateAnswerOptionDto> AnswerOptions { get; set; } = new();
}

public class CreateAnswerOptionDto
{
    [Required]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int OptionOrder { get; set; }
}
