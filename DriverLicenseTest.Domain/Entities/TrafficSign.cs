using System;
using System.Collections.Generic;

namespace DriverLicenseTest.Domain.Entities;

public partial class TrafficSign
{
    public int SignId { get; set; }

    public string SignCode { get; set; } = null!;

    public string SignName { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string SignType { get; set; } = null!;

    public int? CategoryId { get; set; }

    public string? Meaning { get; set; }

    public int RelatedQuestionCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

   
}
