using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public sealed class FileStorageOptions { public string RootPath { get; set; } = "AppData/uploads"; }
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, int userId, int? projectId, string extension, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;
    public LocalFileStorageService(IOptions<FileStorageOptions> options, IWebHostEnvironment environment)
    {
        _root = Path.GetFullPath(Path.IsPathRooted(options.Value.RootPath) ? options.Value.RootPath : Path.Combine(environment.ContentRootPath, options.Value.RootPath));
        Directory.CreateDirectory(_root);
    }
    public async Task<string> SaveAsync(Stream content, int userId, int? projectId, string extension, CancellationToken cancellationToken = default)
    {
        var relative = Path.Combine(userId.ToString(), projectId?.ToString() ?? "personal", $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
        var full = Resolve(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var output = File.Create(full);
        if (content.CanSeek) content.Position = 0;
        await content.CopyToAsync(output, cancellationToken);
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }
    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        try { return Task.FromResult<Stream?>(File.OpenRead(Resolve(relativePath))); } catch (FileNotFoundException) { return Task.FromResult<Stream?>(null); }
    }
    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var full = Resolve(relativePath); if (File.Exists(full)) File.Delete(full); return Task.CompletedTask;
    }
    private string Resolve(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) throw new InvalidOperationException("Invalid file path.");
        var full = Path.GetFullPath(Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid file path.");
        return full;
    }
}
