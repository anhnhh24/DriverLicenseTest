public class UserStatisticDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalExamsTaken { get; set; }
    public int TotalExamsPassed { get; set; }
    public int TotalExamsFailed { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public decimal AccuracyRate { get; set; }
    public int TotalLearningTime { get; set; }
    public decimal AverageScore { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}