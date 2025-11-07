using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    
    public virtual ICollection<MockExam> MockExams { get; set; } = new List<MockExam>();

    public virtual UserStatistic? UserStatistic { get; set; }

    public virtual ICollection<UserWrongQuestion> UserWrongQuestions { get; set; } = new List<UserWrongQuestion>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
