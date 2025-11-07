using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class MockExam
{
    public int ExamId { get; set; }

    public string UserId { get; set; } = null!;

    public int LicenseTypeId { get; set; }

    public int Score { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public int WrongAnswers { get; set; }

    public int? TimeSpent { get; set; }

    public string PassStatus { get; set; } = null!;

    public bool? FailedElimination { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int PassingScore { get; set; }

    public int? RequiredElimination { get; set; }

    public bool IsSubmitted { get; set; }

    public virtual LicenseType LicenseType { get; set; } = null!;

    public virtual ICollection<MockExamAnswer> MockExamAnswers { get; set; } = new List<MockExamAnswer>();

    public virtual AspNetUser User { get; set; } = null!;
}
