using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentActivity
{
    [Key] public int DocumentActivityId { get; set; }
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    public DateTime OccurredDate { get; set; } = DateTime.UtcNow;
    [MaxLength(1000)] public string? Details { get; set; }
    [ForeignKey(nameof(DocumentId))] public virtual Document Document { get; set; } = null!;
    [ForeignKey(nameof(UserId))] public virtual User User { get; set; } = null!;
}
