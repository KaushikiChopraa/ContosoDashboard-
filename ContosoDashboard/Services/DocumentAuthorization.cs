using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public sealed class DocumentAuthorization
{
    private readonly ApplicationDbContext _context;
    public DocumentAuthorization(ApplicationDbContext context) => _context = context;
    public async Task<bool> CanReadAsync(Document document, int userId)
    {
        if (document.UploadedByUserId == userId) return true;
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return false;
        if (user?.Role == UserRole.Administrator) return true;
        if (document.ProjectId.HasValue && await _context.Projects.AnyAsync(p => p.ProjectId == document.ProjectId && (p.ProjectManagerId == userId || p.ProjectMembers.Any(m => m.UserId == userId)))) return true;
        return await _context.DocumentShares.AnyAsync(s => s.DocumentId == document.DocumentId && (s.SharedWithUserId == userId || (s.SharedWithDepartment != null && s.SharedWithDepartment == user!.Department)));
    }
    public async Task<bool> CanManageAsync(Document document, int userId)
    {
        if (document.UploadedByUserId == userId) return true;
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        if (user?.Role == UserRole.Administrator) return true;
        return document.ProjectId.HasValue && await _context.Projects.AnyAsync(p => p.ProjectId == document.ProjectId && p.ProjectManagerId == userId);
    }
    public async Task<bool> CanUploadToProjectAsync(int projectId, int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        return user?.Role == UserRole.Administrator || await _context.Projects.AnyAsync(p => p.ProjectId == projectId && (p.ProjectManagerId == userId || p.ProjectMembers.Any(m => m.UserId == userId)));
    }
}
