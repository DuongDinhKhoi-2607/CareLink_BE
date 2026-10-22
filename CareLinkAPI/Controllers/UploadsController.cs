using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/uploads")]
[Produces("application/json")]
public class UploadsController(IWebHostEnvironment env, ILogger<UploadsController> logger) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".pdf"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Upload file tài liệu / ảnh xác minh (CCCD, Bằng cấp, Chứng chỉ, Thẻ sinh viên).
    /// </summary>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadFileResponse>> UploadFile(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Tệp không hợp lệ",
                Detail = "Vui lòng chọn một tệp hình ảnh hoặc tài liệu để tải lên."
            });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dung lượng vượt quá giới hạn",
                Detail = "Dung lượng tệp tối đa cho phép là 10 MB."
            });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Định dạng không được hỗ trợ",
                Detail = "Hệ thống chỉ chấp nhận định dạng ảnh JPG, JPEG, PNG, WEBP hoặc tệp PDF."
            });
        }

        var webRoot = env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsFolder = Path.Combine(webRoot, "uploads", "documents");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName).Replace(" ", "_")}";
        var destinationPath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var relativeUrl = $"/uploads/documents/{uniqueFileName}";
        var fullUrl = $"{baseUrl}{relativeUrl}";

        logger.LogInformation("File uploaded successfully: {RelativeUrl} ({Size} bytes)", relativeUrl, file.Length);

        return Ok(new UploadFileResponse
        {
            FileName = file.FileName,
            FileUrl = fullUrl,
            RelativeUrl = relativeUrl,
            Size = file.Length,
            ContentType = file.ContentType
        });
    }
}

public class UploadFileResponse
{
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string RelativeUrl { get; set; } = null!;
    public long Size { get; set; }
    public string? ContentType { get; set; }
}
