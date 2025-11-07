using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class LicenseType
{
    public int LicenseTypeId { get; set; }

    public string LicenseCode { get; set; } = null!;

    public string LicenseName { get; set; } = null!;

    public string? VehicleType { get; set; }

    public int TotalQuestions { get; set; }

    public int TimeLimit { get; set; }

    public int PassingScore { get; set; }

    public int? RequiredElimination { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<LicenseQuestion> LicenseQuestions { get; set; } = new List<LicenseQuestion>();

    public virtual ICollection<MockExam> MockExams { get; set; } = new List<MockExam>();
}
