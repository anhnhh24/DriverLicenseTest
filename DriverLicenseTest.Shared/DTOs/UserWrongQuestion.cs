using DriverLicenseTest.Shared.DTOs.MockExam;

public class UserWrongQuestionDto
{
    public int UserWrongQuestionId { get; set; }
    public string UserId { get; set; }
    public int QuestionId { get; set; }
    public int WrongCount { get; set; }
    public bool IsFixed { get; set; }
    public DateTime LastWrongAt { get; set; }

    // Question details
    public string QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }

    // Answer details
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    public int? CorrectOptionId { get; set; }
    public string? CorrectAnswerText { get; set; }
}