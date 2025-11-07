using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuestionNumber { get; set; }

    public int CategoryId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? ExplanationText { get; set; }

    public string? DifficultyLevel { get; set; }

    public bool? IsElimination { get; set; }

    public string? ImageUrl { get; set; }

    public int? TimeLimit { get; set; }

    public int? Points { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<LicenseQuestion> LicenseQuestions { get; set; } = new List<LicenseQuestion>();

    public virtual ICollection<MockExamAnswer> MockExamAnswers { get; set; } = new List<MockExamAnswer>();

    public virtual ICollection<UserWrongQuestion> UserWrongQuestions { get; set; } = new List<UserWrongQuestion>();
}
