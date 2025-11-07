using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class MockExamAnswer
{
    public int ExamAnswerId { get; set; }

    public int ExamId { get; set; }

    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public bool IsCorrect { get; set; }

    public int? TimeSpent { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsElimination { get; set; }

    public bool ReviewedAfterSubmit { get; set; }

    public virtual MockExam Exam { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual AnswerOption? SelectedOption { get; set; }
}
