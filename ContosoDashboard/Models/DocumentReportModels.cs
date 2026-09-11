namespace ContosoDashboard.Models;

public sealed class DocumentReport
{
    public List<DocumentTypeSummary> Types { get; init; } = new();
    public List<UserActivitySummary> Uploaders { get; init; } = new();
    public List<DocumentAccessSummary> AccessPatterns { get; init; } = new();
}
public sealed record DocumentTypeSummary(string FileType, int Count);
public sealed record UserActivitySummary(int UserId, string UserName, int Count);
public sealed record DocumentAccessSummary(string Action, int Count);
