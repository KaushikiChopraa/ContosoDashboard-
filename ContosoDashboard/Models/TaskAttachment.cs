using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class TaskAttachment
{
    [Key] public int TaskAttachmentId { get; set; }
    public int DocumentId { get; set; }
    public int TaskId { get; set; }
    public int AttachedByUserId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(DocumentId))] public virtual Document Document { get; set; } = null!;
    [ForeignKey(nameof(TaskId))] public virtual TaskItem Task { get; set; } = null!;
    [ForeignKey(nameof(AttachedByUserId))] public virtual User AttachedByUser { get; set; } = null!;
}
