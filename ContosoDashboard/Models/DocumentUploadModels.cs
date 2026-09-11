namespace ContosoDashboard.Models;

public sealed class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public int? ProjectId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Stream Content { get; set; } = Stream.Null;
}

public sealed class DocumentUploadResult
{
    public bool Success { get; init; }
    public Document? Document { get; init; }
    public string? Error { get; init; }
    public string FileName { get; init; } = string.Empty;
}

public sealed class DocumentQuery
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public int? ProjectId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "date";
    public bool Descending { get; set; } = true;
}

public sealed class DocumentFileResult
{
    public Stream Content { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
}
