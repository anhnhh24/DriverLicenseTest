namespace DriverLicenseTest.Shared.DTOs.UserWrongQuestion;

public class UserWrongQuestionDto
{
    public int WrongQuestionId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public int WrongCount { get; set; }
    public DateTime LastWrongAt { get; set; }
    public bool IsFixed { get; set; }
}

public class AddWrongQuestionDto
{
    public int QuestionId { get; set; }
}

public class FixWrongQuestionDto
{
    public int WrongQuestionId { get; set; }
}
