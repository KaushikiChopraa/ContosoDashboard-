using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Xunit;

namespace ContosoDashboard.Tests;

public class DocumentValidationTests
{
    [Fact]
    public void Accepts_supported_file_and_category()
    {
        var error = DocumentValidation.Validate(new DocumentUploadRequest { Title = "Report", Category = "Reports", OriginalFileName = "report.pdf", ContentType = "application/pdf", FileSize = 100 });
        Assert.Null(error);
    }

    [Fact]
    public void Rejects_oversized_file()
    {
        var error = DocumentValidation.Validate(new DocumentUploadRequest { Title = "Report", Category = "Reports", OriginalFileName = "report.pdf", ContentType = "application/pdf", FileSize = DocumentValidation.MaxFileSize + 1 });
        Assert.Contains("25 MB", error);
    }

    [Fact]
    public void Rejects_unsupported_extension()
    {
        var error = DocumentValidation.Validate(new DocumentUploadRequest { Title = "Binary", Category = "Other", OriginalFileName = "binary.exe", ContentType = "application/octet-stream", FileSize = 100 });
        Assert.Contains("not supported", error);
    }

    [Fact]
    public void Normalizes_tags()
    {
        Assert.Equal("one, two", DocumentValidation.NormalizeTags("one, two, one"));
    }
}
