using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class AnswerOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int OptionOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MockExamAnswer> MockExamAnswers { get; set; } = new List<MockExamAnswer>();

    public virtual Question Question { get; set; } = null!;
}
