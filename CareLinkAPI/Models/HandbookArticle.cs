using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public class HandbookReferenceItem
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public partial class HandbookArticle
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string ReadTime { get; set; } = "5 phút đọc";

    public string Source { get; set; } = "Vinmec";

    public string? SourceDetail { get; set; }

    public string? ImageUrl { get; set; }

    public bool Featured { get; set; }

    /// <summary>Trạng thái: "published" | "draft" | "archived"</summary>
    public string Status { get; set; } = "published";

    public List<HandbookReferenceItem> References { get; set; } = new();

    public Guid? AuthorId { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? Author { get; set; }
}
