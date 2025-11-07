namespace DriverLicenseTest.Shared.DTOs.TrafficSign;

public class TrafficSignDto
{
    public int SignId { get; set; }
    public string SignCode { get; set; }
    public string SignName { get; set; }
    public string Description { get; set; }
    public string ImageURL { get; set; }

    /// <summary>
    /// SignType in English from database
    /// </summary>
    public string SignType { get; set; }

    /// <summary>
    /// SignType in Vietnamese for display
    /// </summary>
    public string SignTypeVietnamese => ConvertSignTypeToVietnamese(SignType);

    public string Meaning { get; set; }
    public int RelatedQuestionCount { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Convert English SignType to Vietnamese
    /// </summary>
    private string ConvertSignTypeToVietnamese(string signType)
    {
        return signType switch
        {
            "Prohibition" => "Biển cấm",
            "Warning" => "Biển cảnh báo",
            "Mandatory" => "Biển hiệu lệnh",
            "Information" => "Biển chỉ dẫn",
            "Additional" => "Biển phụ",
            _ => signType // Return original if not matched
        };
    }
}
