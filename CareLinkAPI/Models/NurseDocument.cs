using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class NurseDocument
{
    public Guid Id { get; set; }

    public Guid NurseId { get; set; }

    public int DocumentType { get; set; }

    public string FileUrl { get; set; } = null!;

    public int Status { get; set; }

    public string? AdminNote { get; set; }

    public string? AdminChecklist { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;

    public virtual User? ReviewedByNavigation { get; set; }
}
