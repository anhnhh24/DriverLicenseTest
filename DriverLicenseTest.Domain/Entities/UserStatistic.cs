using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class UserStatistic
{
    public int StatisticId { get; set; }

    public string UserId { get; set; } = null!;

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

    public DateTime? LastActivityAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
