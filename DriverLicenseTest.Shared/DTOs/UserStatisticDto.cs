namespace DriverLicenseTest.Shared.DTOs.UserStatistic;

public class UserStatisticDto
{
    public int StatisticId { get; set; }
    public string UserId { get; set; }
    public int TotalExamsTaken { get; set; }
    public int TotalExamsPassed { get; set; }
    public int TotalExamsFailed { get; set; }
    public decimal AverageScore { get; set; }
    public int HighestScore { get; set; }
    public int LowestScore { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public decimal AccuracyRate { get; set; }
    public int TotalLearningTime { get; set; }
    public int PassRate => TotalExamsTaken > 0 ? (TotalExamsPassed * 100 / TotalExamsTaken) : 0;
}
