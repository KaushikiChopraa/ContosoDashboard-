using ContosoDashboard.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace ContosoDashboard.Tests;

public class FileStorageServiceTests
{
    [Fact]
    public async Task Saves_under_generated_user_path_and_reads_content()
    {
        var root = Path.Combine(Path.GetTempPath(), "contoso-doc-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var service = new LocalFileStorageService(Options.Create(new FileStorageOptions { RootPath = root }), new TestEnvironment());
            await using var input = new MemoryStream("hello"u8.ToArray());
            var path = await service.SaveAsync(input, 4, null, ".txt");
            Assert.StartsWith("4/personal/", path);
            await using (var output = await service.OpenReadAsync(path))
            using (var reader = new StreamReader(output!))
            {
                Assert.Equal("hello", await reader.ReadToEndAsync());
            }
            await service.DeleteAsync(path);
            Assert.Null(await service.OpenReadAsync(path));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private sealed class TestEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
