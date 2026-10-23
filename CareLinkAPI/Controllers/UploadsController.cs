using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/uploads")]
[Produces("application/json")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<UploadsController> _logger;
    private readonly string _connectionString;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".pdf"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadsController(
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<UploadsController> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    /// <summary>
    /// Upload file tài liệu / ảnh xác minh (CCCD, Bằng cấp, Chứng chỉ, Thẻ sinh viên).
    /// Tự động lưu trữ vĩnh viễn vào Supabase PostgreSQL (Persistent Storage) và Disk Cache.
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

        var uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName).Replace(" ", "_")}";
        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            contentType = GetMimeType(ext);
        }

        // 1. Đọc toàn bộ nội dung file ra byte array
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);
        var fileBytes = memoryStream.ToArray();

        // 2. Lưu trữ vĩnh viễn vào Supabase PostgreSQL (bảo toàn 100% dữ liệu ngay cả khi Render restart/sleep)
        if (!string.IsNullOrWhiteSpace(_connectionString))
        {
            try
            {
                await using var conn = new Npgsql.NpgsqlConnection(_connectionString);
                await conn.OpenAsync(ct);
                const string sql = @"
                    INSERT INTO ""uploaded_files"" (""file_name"", ""content_type"", ""data"", ""size_bytes"")
                    VALUES (@fileName, @contentType, @data, @sizeBytes)
                    ON CONFLICT (""file_name"") DO UPDATE 
                    SET ""data"" = EXCLUDED.""data"", ""content_type"" = EXCLUDED.""content_type"";";
                await using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("fileName", uniqueFileName);
                cmd.Parameters.AddWithValue("contentType", contentType);
                cmd.Parameters.AddWithValue("data", fileBytes);
                cmd.Parameters.AddWithValue("sizeBytes", file.Length);
                await cmd.ExecuteNonQueryAsync(ct);
                _logger.LogInformation("File persisted to Supabase DB: {FileName} ({Size} bytes)", uniqueFileName, file.Length);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lưu trữ file vào Supabase Database, fallback sang ổ đĩa cục bộ.");
            }
        }

        // 3. Ghi song song vào Disk Cache (nếu có thư mục cục bộ) để phục vụ nhanh
        try
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRoot, "uploads", "documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var destinationPath = Path.Combine(uploadsFolder, uniqueFileName);
            await System.IO.File.WriteAllBytesAsync(destinationPath, fileBytes, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi ghi disk cache cho file {FileName}", uniqueFileName);
        }

        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var relativeUrl = $"/uploads/documents/{uniqueFileName}";
        var fullUrl = $"{baseUrl}{relativeUrl}";

        _logger.LogInformation("File uploaded successfully: {RelativeUrl} ({Size} bytes)", relativeUrl, file.Length);

        return Ok(new UploadFileResponse
        {
            FileName = file.FileName,
            FileUrl = fullUrl,
            RelativeUrl = relativeUrl,
            Size = file.Length,
            ContentType = contentType
        });
    }

    /// <summary>
    /// Phục vụ trực tiếp file đã tải lên theo đường dẫn /uploads/documents/{fileName}.
    /// Hỗ trợ cả file trên ổ cứng lẫn file được bảo toàn trong Supabase PostgreSQL.
    /// </summary>
    [HttpGet("/uploads/documents/{fileName}")]
    [HttpGet("{fileName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFile(string fileName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return NotFound();

        var safeFileName = Path.GetFileName(fileName);
        var ext = Path.GetExtension(safeFileName);
        var mimeType = GetMimeType(ext);

        // 1. Kiểm tra trên Disk Cache trước
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var localPath = Path.Combine(webRoot, "uploads", "documents", safeFileName);
        if (System.IO.File.Exists(localPath))
        {
            return PhysicalFile(localPath, mimeType);
        }

        // 2. Fallback: Lấy trực tiếp từ Supabase PostgreSQL (vĩnh viễn)
        if (!string.IsNullOrWhiteSpace(_connectionString))
        {
            try
            {
                await using var conn = new Npgsql.NpgsqlConnection(_connectionString);
                await conn.OpenAsync(ct);
                const string sql = @"SELECT ""content_type"", ""data"" FROM ""uploaded_files"" WHERE ""file_name"" = @fileName LIMIT 1;";
                await using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("fileName", safeFileName);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    var dbContentType = reader.GetString(0);
                    var data = (byte[])reader[1];
                    if (string.IsNullOrWhiteSpace(dbContentType)) dbContentType = mimeType;

                    // Ghi lại vào disk cache cho các lần gọi sau
                    try
                    {
                        var dir = Path.GetDirectoryName(localPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        await System.IO.File.WriteAllBytesAsync(localPath, data, ct);
                    }
                    catch
                    {
                        // Bỏ qua lỗi disk cache
                    }

                    return File(data, dbContentType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy xuất file {FileName} từ Supabase Database", safeFileName);
            }
        }

        return NotFound(new ProblemDetails
        {
            Title = "Không tìm thấy tệp",
            Detail = $"Tệp tin '{safeFileName}' không tồn tại hoặc đã bị xóa."
        });
    }

    private static string GetMimeType(string ext) => ext.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}

public class UploadFileResponse
{
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string RelativeUrl { get; set; } = null!;
    public long Size { get; set; }
    public string? ContentType { get; set; }
}
