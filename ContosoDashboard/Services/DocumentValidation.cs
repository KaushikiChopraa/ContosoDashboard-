using System.IO;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public static class DocumentValidation
{
    public const long MaxFileSize = 25 * 1024 * 1024;
    public static readonly IReadOnlySet<string> Categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other" };
    public static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf", [".doc"] = "application/msword", [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel", [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".ppt"] = "application/vnd.ms-powerpoint", [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".txt"] = "text/plain", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png"
    };

    public static string? Validate(DocumentUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 255) return "A title between 1 and 255 characters is required.";
        if (!Categories.Contains(request.Category)) return "Select a valid document category.";
        if (request.FileSize <= 0 || request.FileSize > MaxFileSize) return "Files must be larger than zero and no more than 25 MB.";
        if (string.IsNullOrWhiteSpace(request.OriginalFileName)) return "A file name is required.";
        var extension = Path.GetExtension(request.OriginalFileName);
        if (!ContentTypes.ContainsKey(extension)) return "This file type is not supported.";
        if (string.IsNullOrWhiteSpace(request.ContentType) || request.ContentType.Length > 255) return "The file type is invalid.";
        return null;
    }

    public static string NormalizeTags(string? tags) => string.Join(", ", (tags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase));
}
