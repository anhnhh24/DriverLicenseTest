namespace DriverLicenseTest.Shared.DTOs.Question;

public class QuestionDto
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string? ExplanationText { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty; // 1=Easy, 2=Medium, 3=Hard
    public bool IsElimination { get; set; } // Câu điểm liệt
    public string? ImageURL { get; set; }
    public int TimeLimit { get; set; } = 30; // seconds
    public int Points { get; set; } = 1;
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
}

public class AnswerOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OptionOrder { get; set; } // 1=A, 2=B, 3=C
}
