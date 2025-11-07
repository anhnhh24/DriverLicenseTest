public class ExamStructure
{
    public string LicenseType { get; set; }
    public int TotalQuestions { get; set; }
    public int PassingScore { get; set; }
    public Dictionary<int, int> Structure { get; set; } // CategoryId -> Number of questions
}

public class ExamStructuresOptions
{
    public List<ExamStructure> ExamStructures { get; set; } = new();
}