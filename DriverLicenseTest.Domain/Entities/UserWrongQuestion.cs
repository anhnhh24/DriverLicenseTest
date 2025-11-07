using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class UserWrongQuestion
{
    public int WrongQuestionId { get; set; }

    public string UserId { get; set; } = null!;

    public int QuestionId { get; set; }

    public int WrongCount { get; set; }

    public DateTime LastWrongAt { get; set; }

    public bool IsFixed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
