namespace DriverLicenseTest.Shared.DTOs.UserPractice;

public class UserPracticeDto
{
    public int PracticeId { get; set; }
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; }
    public int CategoryId { get; set; } 
    public string CategoryName { get; set; }
    public int? SelectedOptionId { get; set; }
    public string SelectedOptionText { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsMarked { get; set; }
    public int TimeSpent { get; set; }
    public int ReviewCount { get; set; }
    public DateTime PracticedAt { get; set; }
}

public class CreateUserPracticeDto
{
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public int TimeSpent { get; set; }
}

public class UpdateUserPracticeDto
{
    public bool IsMarked { get; set; }
}
