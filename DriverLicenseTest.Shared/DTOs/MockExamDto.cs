using DriverLicenseTest.Shared.DTOs.Question;

namespace DriverLicenseTest.Shared.DTOs.MockExam;

public class MockExamDto
{
    public int ExamId { get; set; }
    public string UserId { get; set; }
    public int LicenseTypeId { get; set; }
    public string LicenseTypeName { get; set; }

    // Exam structure
    public int TotalQuestions { get; set; }
    public int PassingScore { get; set; }

    // Results
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int? Score { get; set; }
    public string PassStatus { get; set; } // "InProgress", "Passed", "Failed"

    // Status
    public bool IsSubmitted { get; set; }

    // Timing
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TimeSpent { get; set; } // seconds

    // Questions (for displaying)
    public List<MockExamQuestionDto> Questions { get; set; } = new();

    // Metadata
    public DateTime CreatedAt { get; set; }
}

public class MockExamQuestionDto
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    public int? SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
}

public class AnswerOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; }
    public bool IsCorrect { get; set; } // Chỉ show sau khi submit
}

public class CreateMockExamDto
{
    public string LicenseType { get; set; } // "B1", "B2", "C", etc.
}

public class SubmitMockExamAnswerDto
{
    public int ExamId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
    public int TimeSpent { get; set; } // seconds for this question
}
