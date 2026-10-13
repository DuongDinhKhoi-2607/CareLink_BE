using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Handbook;

public class HandbookReferenceDto
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// DTO trả về cho danh sách bài viết Public (chỉ published).
/// Bao gồm cả property Image để khớp 100% với FE data binding (MedicalHandbook.jsx).
/// </summary>
public class HandbookArticleListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ReadTime { get; set; } = "5 phút đọc";
    public string Source { get; set; } = "Vinmec";
    public string? SourceDetail { get; set; }
    public string? ImageUrl { get; set; }
    public string? Image => ImageUrl; // FE compatibility alias
    public bool Featured { get; set; }
    public string Status { get; set; } = "published";
    public DateTime? PublishedAt { get; set; }
    public string Date => PublishedAt?.ToString("dd/MM/yyyy") ?? CreatedAt.ToString("dd/MM/yyyy");
    public DateTime CreatedAt { get; set; }
    public int ReferencesCount { get; set; }
}

/// <summary>
/// DTO trả về cho chi tiết bài viết (Public & Preview).
/// </summary>
public class HandbookArticleDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ReadTime { get; set; } = "5 phút đọc";
    public string Source { get; set; } = "Vinmec";
    public string? SourceDetail { get; set; }
    public string? ImageUrl { get; set; }
    public string? Image => ImageUrl; // FE compatibility alias
    public bool Featured { get; set; }
    public string Status { get; set; } = "published";
    public List<HandbookReferenceDto> References { get; set; } = new();
    public Guid? AuthorId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string Date => PublishedAt?.ToString("dd/MM/yyyy") ?? CreatedAt.ToString("dd/MM/yyyy");
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO trả về cho danh sách quản trị Admin.
/// </summary>
public class HandbookAdminListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ReadTime { get; set; } = "5 phút đọc";
    public string Source { get; set; } = "Vinmec";
    public string? SourceDetail { get; set; }
    public string? ImageUrl { get; set; }
    public string? Image => ImageUrl;
    public bool Featured { get; set; }
    public string Status { get; set; } = "published";
    public int ReferencesCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request tạo bài viết mới từ Admin.
/// </summary>
public class CreateHandbookArticleDto
{
    [Required(ErrorMessage = "Tiêu đề bài viết không được để trống")]
    [MaxLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    [Required(ErrorMessage = "Chuyên mục không được để trống")]
    public string Category { get; set; } = "elderly";

    public string? CategoryName { get; set; }

    [Required(ErrorMessage = "Tóm tắt không được để trống")]
    [MaxLength(500, ErrorMessage = "Tóm tắt tối đa 500 ký tự")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
    public string Content { get; set; } = string.Empty;

    public string? ReadTime { get; set; } = "5 phút đọc";

    public string? Source { get; set; } = "Vinmec";

    public string? SourceDetail { get; set; }

    public string? ImageUrl { get; set; }
    public string? Image { set => ImageUrl = value; } // Cho phép FE gửi field 'image' hoặc 'imageUrl'

    public bool Featured { get; set; } = false;

    public string Status { get; set; } = "published";

    public List<HandbookReferenceDto>? References { get; set; }
}

/// <summary>
/// Request cập nhật bài viết từ Admin.
/// </summary>
public class UpdateHandbookArticleDto
{
    [Required(ErrorMessage = "Tiêu đề bài viết không được để trống")]
    [MaxLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    [Required(ErrorMessage = "Chuyên mục không được để trống")]
    public string Category { get; set; } = "elderly";

    public string? CategoryName { get; set; }

    [Required(ErrorMessage = "Tóm tắt không được để trống")]
    [MaxLength(500, ErrorMessage = "Tóm tắt tối đa 500 ký tự")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
    public string Content { get; set; } = string.Empty;

    public string? ReadTime { get; set; }

    public string? Source { get; set; }

    public string? SourceDetail { get; set; }

    public string? ImageUrl { get; set; }
    public string? Image { set => ImageUrl = value; }

    public bool Featured { get; set; }

    public string Status { get; set; } = "published";

    public List<HandbookReferenceDto>? References { get; set; }
}

public class UpdateHandbookStatusDto
{
    [Required]
    public string Status { get; set; } = "published"; // published | draft | archived
}

public class UpdateHandbookFeaturedDto
{
    public bool Featured { get; set; }
}

/// <summary>
/// Query filter phân trang cho danh sách bài viết.
/// </summary>
public class HandbookQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Category { get; set; }
    public string? Search { get; set; }
    public bool? Featured { get; set; }
    public string? Status { get; set; }
}
