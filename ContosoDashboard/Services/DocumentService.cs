using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentUploadResult>> UploadAsync(int userId, IEnumerable<DocumentUploadRequest> requests, CancellationToken cancellationToken = default);
    Task<List<Document>> SearchAsync(int userId, DocumentQuery query, CancellationToken cancellationToken = default);
    Task<DocumentFileResult?> OpenAsync(int userId, int documentId, bool preview, CancellationToken cancellationToken = default);
    Task<bool> UpdateMetadataAsync(int userId, int documentId, string title, string? description, string category, string? tags);
    Task<bool> ReplaceFileAsync(int userId, int documentId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default);
    Task<bool> ShareAsync(int userId, int documentId, DocumentShareRequest request);
    Task<List<Document>> GetSharedWithMeAsync(int userId);
    Task<bool> AttachToTaskAsync(int userId, int documentId, int taskId);
    Task<DocumentReport?> GetReportAsync(int userId);
}

public sealed class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly IFileSecurityScanner _scanner;
    private readonly DocumentAuthorization _authorization;
    private readonly INotificationService _notifications;

    public DocumentService(ApplicationDbContext context, IFileStorageService storage, IFileSecurityScanner scanner, DocumentAuthorization authorization, INotificationService notifications)
    { _context = context; _storage = storage; _scanner = scanner; _authorization = authorization; _notifications = notifications; }

    public async Task<IReadOnlyList<DocumentUploadResult>> UploadAsync(int userId, IEnumerable<DocumentUploadRequest> requests, CancellationToken cancellationToken = default)
    {
        var results = new List<DocumentUploadResult>();
        foreach (var request in requests)
        {
            var error = DocumentValidation.Validate(request);
            if (error == null && request.ProjectId.HasValue && !await _authorization.CanUploadToProjectAsync(request.ProjectId.Value, userId)) error = "You are not authorized to upload to this project.";
            if (error != null) { results.Add(new() { FileName = request.OriginalFileName, Error = error }); continue; }
            var safe = await _scanner.IsSafeAsync(request.Content, cancellationToken);
            if (!safe) { results.Add(new() { FileName = request.OriginalFileName, Error = "The file could not pass the security check." }); continue; }
            string? path = null;
            try
            {
                path = await _storage.SaveAsync(request.Content, userId, request.ProjectId, Path.GetExtension(request.OriginalFileName), cancellationToken);
                var document = new Document { Title = request.Title.Trim(), Description = request.Description?.Trim(), Category = request.Category.Trim(), Tags = DocumentValidation.NormalizeTags(request.Tags), OriginalFileName = Path.GetFileName(request.OriginalFileName), FilePath = path, FileSize = request.FileSize, FileType = request.ContentType, UploadedByUserId = userId, ProjectId = request.ProjectId, UploadedDate = DateTime.UtcNow };
                _context.Documents.Add(document);
                await _context.SaveChangesAsync(cancellationToken);
                await AddActivityAsync(document.DocumentId, userId, "upload", cancellationToken);
                if (request.ProjectId.HasValue) await NotifyProjectAsync(request.ProjectId.Value, userId, document.Title);
                results.Add(new() { Success = true, Document = document, FileName = document.OriginalFileName });
            }
            catch
            {
                if (path != null) await _storage.DeleteAsync(path, cancellationToken);
                results.Add(new() { FileName = request.OriginalFileName, Error = "The file could not be stored." });
            }
        }
        return results;
    }

    public async Task<List<Document>> SearchAsync(int userId, DocumentQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null) return new();
        var isAdmin = user.Role == UserRole.Administrator;
        var documents = _context.Documents.AsNoTracking().Include(d => d.UploadedByUser).Include(d => d.Project).Include(d => d.Shares).AsQueryable();
        if (!isAdmin)
        {
            documents = documents.Where(d => d.UploadedByUserId == userId || (d.ProjectId.HasValue && _context.Projects.Any(p => p.ProjectId == d.ProjectId && (p.ProjectManagerId == userId || p.ProjectMembers.Any(m => m.UserId == userId)))) || d.Shares.Any(s => s.SharedWithUserId == userId || (s.SharedWithDepartment != null && s.SharedWithDepartment == user.Department)));
        }
        if (!string.IsNullOrWhiteSpace(query.Search)) { var term = query.Search.Trim(); documents = documents.Where(d => d.Title.Contains(term) || (d.Description != null && d.Description.Contains(term)) || (d.Tags != null && d.Tags.Contains(term)) || d.UploadedByUser.DisplayName.Contains(term) || (d.Project != null && d.Project.Name.Contains(term))); }
        if (!string.IsNullOrWhiteSpace(query.Category)) documents = documents.Where(d => d.Category == query.Category);
        if (query.ProjectId.HasValue) documents = documents.Where(d => d.ProjectId == query.ProjectId);
        if (query.FromDate.HasValue) documents = documents.Where(d => d.UploadedDate >= query.FromDate.Value);
        if (query.ToDate.HasValue) documents = documents.Where(d => d.UploadedDate < query.ToDate.Value.Date.AddDays(1));
        documents = query.SortBy.ToLowerInvariant() switch { "title" => query.Descending ? documents.OrderByDescending(d => d.Title) : documents.OrderBy(d => d.Title), "category" => query.Descending ? documents.OrderByDescending(d => d.Category) : documents.OrderBy(d => d.Category), "size" => query.Descending ? documents.OrderByDescending(d => d.FileSize) : documents.OrderBy(d => d.FileSize), _ => query.Descending ? documents.OrderByDescending(d => d.UploadedDate) : documents.OrderBy(d => d.UploadedDate) };
        return await documents.Take(500).ToListAsync(cancellationToken);
    }

    public async Task<DocumentFileResult?> OpenAsync(int userId, int documentId, bool preview, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
        if (document == null || !await _authorization.CanReadAsync(document, userId)) return null;
        if (preview && document.FileType is not ("application/pdf" or "image/jpeg" or "image/png")) return null;
        var stream = await _storage.OpenReadAsync(document.FilePath, cancellationToken);
        if (stream == null) return null;
        await AddActivityAsync(documentId, userId, preview ? "preview" : "download", cancellationToken);
        return new DocumentFileResult { Content = stream, FileName = document.OriginalFileName, ContentType = document.FileType };
    }

    public async Task<bool> UpdateMetadataAsync(int userId, int documentId, string title, string? description, string category, string? tags)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId); if (document == null || !await _authorization.CanManageAsync(document, userId)) return false;
        var request = new DocumentUploadRequest { Title = title, Category = category, Description = description, Tags = tags, OriginalFileName = document.OriginalFileName, ContentType = document.FileType, FileSize = document.FileSize };
        if (!string.IsNullOrEmpty(DocumentValidation.Validate(request))) return false;
        document.Title = title.Trim(); document.Description = description?.Trim(); document.Category = category.Trim(); document.Tags = DocumentValidation.NormalizeTags(tags); await _context.SaveChangesAsync(); await AddActivityAsync(documentId, userId, "edit", CancellationToken.None); return true;
    }

    public async Task<bool> ReplaceFileAsync(int userId, int documentId, DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken); if (document == null || !await _authorization.CanManageAsync(document, userId) || DocumentValidation.Validate(request) != null || !await _scanner.IsSafeAsync(request.Content, cancellationToken)) return false;
        var oldPath = document.FilePath; var newPath = await _storage.SaveAsync(request.Content, userId, document.ProjectId, Path.GetExtension(request.OriginalFileName), cancellationToken);
        try { document.FilePath = newPath; document.OriginalFileName = Path.GetFileName(request.OriginalFileName); document.FileSize = request.FileSize; document.FileType = request.ContentType; await _context.SaveChangesAsync(cancellationToken); await _storage.DeleteAsync(oldPath, cancellationToken); await AddActivityAsync(documentId, userId, "replace", cancellationToken); return true; }
        catch { await _storage.DeleteAsync(newPath, cancellationToken); return false; }
    }

    public async Task<bool> DeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken); if (document == null || !await _authorization.CanManageAsync(document, userId)) return false;
        await _storage.DeleteAsync(document.FilePath, cancellationToken); _context.Documents.Remove(document); await _context.SaveChangesAsync(cancellationToken); _context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, UserId = userId, Action = "delete", Details = document.Title }); await _context.SaveChangesAsync(cancellationToken); return true;
    }

    public async Task<bool> ShareAsync(int userId, int documentId, DocumentShareRequest request)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId); if (document == null || document.UploadedByUserId != userId) return false;
        if ((request.UserId.HasValue) == string.IsNullOrWhiteSpace(request.Department)) return false;
        if (!string.IsNullOrWhiteSpace(request.Department) && document.Category != "Team Resources") return false;
        if (request.Department != null && document.ProjectId.HasValue) return false;
        if (request.UserId.HasValue && await _context.DocumentShares.AnyAsync(s => s.DocumentId == documentId && s.SharedWithUserId == request.UserId)) return false;
        if (request.Department != null && await _context.DocumentShares.AnyAsync(s => s.DocumentId == documentId && s.SharedWithDepartment == request.Department)) return false;
        var share = new DocumentShare { DocumentId = documentId, SharedWithUserId = request.UserId, SharedWithDepartment = request.Department, SharedByUserId = userId }; _context.DocumentShares.Add(share); await _context.SaveChangesAsync(); await AddActivityAsync(documentId, userId, "share", CancellationToken.None);
        if (request.UserId.HasValue) await _notifications.CreateNotificationAsync(new Notification { UserId = request.UserId.Value, Title = "Document shared with you", Message = $"A document was shared with you: {document.Title}", Type = NotificationType.SystemAnnouncement });
        return true;
    }

    public Task<List<Document>> GetSharedWithMeAsync(int userId) => _context.Documents.Include(d => d.UploadedByUser).Include(d => d.Project).Where(d => d.Shares.Any(s => s.SharedWithUserId == userId)).OrderByDescending(d => d.UploadedDate).ToListAsync();

    public async Task<bool> AttachToTaskAsync(int userId, int documentId, int taskId)
    {
        var task = await _context.Tasks.Include(t => t.Project).ThenInclude(p => p!.ProjectMembers).FirstOrDefaultAsync(t => t.TaskId == taskId); var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId); if (task == null || document == null || !await _authorization.CanReadAsync(document, userId)) return false;
        var taskAccess = task.AssignedUserId == userId || task.CreatedByUserId == userId || task.Project?.ProjectManagerId == userId || task.Project?.ProjectMembers.Any(m => m.UserId == userId) == true; if (!taskAccess || document.ProjectId != task.ProjectId || await _context.TaskAttachments.AnyAsync(a => a.TaskId == taskId && a.DocumentId == documentId)) return false;
        _context.TaskAttachments.Add(new TaskAttachment { TaskId = taskId, DocumentId = documentId, AttachedByUserId = userId }); await _context.SaveChangesAsync(); await AddActivityAsync(documentId, userId, "attach", CancellationToken.None); return true;
    }

    public async Task<DocumentReport?> GetReportAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId); if (user?.Role != UserRole.Administrator) return null;
        return new DocumentReport { Types = await _context.Documents.GroupBy(d => d.FileType).Select(g => new DocumentTypeSummary(g.Key, g.Count())).ToListAsync(), Uploaders = await _context.Documents.GroupBy(d => new { d.UploadedByUserId, d.UploadedByUser.DisplayName }).Select(g => new UserActivitySummary(g.Key.UploadedByUserId, g.Key.DisplayName, g.Count())).ToListAsync(), AccessPatterns = await _context.DocumentActivities.GroupBy(a => a.Action).Select(g => new DocumentAccessSummary(g.Key, g.Count())).ToListAsync() };
    }

    private async Task AddActivityAsync(int documentId, int userId, string action, CancellationToken cancellationToken) { _context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, UserId = userId, Action = action }); await _context.SaveChangesAsync(cancellationToken); }
    private async Task NotifyProjectAsync(int projectId, int actorId, string title) { var ids = await _context.ProjectMembers.Where(m => m.ProjectId == projectId && m.UserId != actorId).Select(m => m.UserId).ToListAsync(); foreach (var id in ids) await _notifications.CreateNotificationAsync(new Notification { UserId = id, Title = "New project document", Message = $"A new document was added: {title}", Type = NotificationType.ProjectUpdate }); }
}
