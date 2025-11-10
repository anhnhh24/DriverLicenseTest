using System.ComponentModel.DataAnnotations;

namespace DriverLicenseTest.Shared.DTOs;

public class CreateQuestionDto
{
    [Required]
    public int QuestionNumber { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public string? ExplanationText { get; set; }

    public string? DifficultyLevel { get; set; }

    public bool IsElimination { get; set; }

    public int TimeLimit { get; set; }

    [Required]
    public List<CreateAnswerOptionDto> AnswerOptions { get; set; } = new();
}

public class CreateAnswerOptionDto
{
    [Required]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int OptionOrder { get; set; }
}