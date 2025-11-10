
public class SubmitExamRequest
{
    public int ExamId { get; set; }
    public int TimeSpent { get; set; } 
    public List<AnswerSubmission> Answers { get; set; }
}
public class AnswerSubmission
{
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; } 
}