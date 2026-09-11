namespace ContosoDashboard.Services;

public interface IFileSecurityScanner
{
    Task<bool> IsSafeAsync(Stream content, CancellationToken cancellationToken = default);
}

public sealed class OfflineFileSecurityScanner : IFileSecurityScanner
{
    public async Task<bool> IsSafeAsync(Stream content, CancellationToken cancellationToken = default)
    {
        if (!content.CanRead) return false;
        if (content.CanSeek) content.Position = 0;
        var buffer = new byte[4096];
        while (await content.ReadAsync(buffer, cancellationToken) > 0) { }
        if (content.CanSeek) content.Position = 0;
        return true;
    }
}
