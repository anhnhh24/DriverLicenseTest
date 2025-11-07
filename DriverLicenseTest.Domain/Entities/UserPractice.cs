using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class UserPractice
{
    public int PracticeId { get; set; }

    public string UserId { get; set; } = null!;

    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public bool IsCorrect { get; set; }

    public int? TimeSpent { get; set; }

    public DateTime PracticedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsMarked { get; set; }

    public int ReviewCount { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual AnswerOption? SelectedOption { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
