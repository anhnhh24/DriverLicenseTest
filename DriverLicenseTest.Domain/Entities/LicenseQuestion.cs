using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class LicenseQuestion
{
    public int LicenseTypeId { get; set; }

    public int QuestionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual LicenseType LicenseType { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
