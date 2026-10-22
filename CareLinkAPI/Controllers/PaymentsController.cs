using System.Text.Json;
using CareLinkAPI.DTOs.Payments;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace CareLinkAPI.Controllers;

/// <summary>Module 7: PayOS payments for bookings.</summary>
[ApiController]
[Produces("application/json")]
public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions WebhookJson = new(JsonSerializerDefaults.Web);

    /// <summary>Pay-01: Khách hàng tạo link thanh toán PayOS cho booking đang chờ thanh toán.</summary>
    [HttpPost("api/v1/bookings/{bookingId:guid}/payments")]
    [ProducesResponseType(typeof(CreatePaymentLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatePaymentLinkResponse>> CreatePaymentLink(
        Guid bookingId,
        [FromQuery] string? returnUrl,
        [FromQuery] string? cancelUrl,
        CancellationToken ct)
    {
        var clientOrigin = Request.Headers.Origin.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(clientOrigin))
        {
            var referer = Request.Headers.Referer.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refUri))
            {
                clientOrigin = $"{refUri.Scheme}://{refUri.Authority}";
            }
        }

        var effectiveReturnUrl = returnUrl;
        var effectiveCancelUrl = cancelUrl;

        if (string.IsNullOrWhiteSpace(effectiveReturnUrl) && !string.IsNullOrWhiteSpace(clientOrigin))
        {
            effectiveReturnUrl = $"{clientOrigin.TrimEnd('/')}/payment/success";
        }

        if (string.IsNullOrWhiteSpace(effectiveCancelUrl) && !string.IsNullOrWhiteSpace(clientOrigin))
        {
            effectiveCancelUrl = $"{clientOrigin.TrimEnd('/')}/payment/cancel";
        }

        var result = await paymentService.CreatePaymentLinkAsync(
            this.GetCurrentUserId(), bookingId, effectiveReturnUrl, effectiveCancelUrl, ct);
        return Ok(result);
    }

    /// <summary>Xem thông tin thanh toán của booking (khách hàng sở hữu hoặc admin).</summary>
    [HttpGet("api/v1/bookings/{bookingId:guid}/payments")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> GetByBooking(Guid bookingId, CancellationToken ct)
    {
        var result = await paymentService.GetByBookingAsync(this.GetCurrentUserId(), bookingId, ct);
        return Ok(result);
    }

    /// <summary>Pay-02: Webhook PayOS gọi về khi có giao dịch. Xác thực chữ ký trước khi xử lý.</summary>
    [AllowAnonymous]
    [HttpPost("api/v1/payments/payos/webhook")]
    public async Task<IActionResult> PayOsWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var raw = await reader.ReadToEndAsync(ct);

        Webhook? webhook;
        try
        {
            webhook = JsonSerializer.Deserialize<Webhook>(raw, WebhookJson);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Malformed PayOS webhook payload.");
            return BadRequest(new { success = false, message = "Malformed payload." });
        }

        if (webhook?.Data is null)
        {
            return BadRequest(new { success = false, message = "Missing webhook data." });
        }

        await paymentService.HandlePayOsWebhookAsync(webhook, raw, ct);
        return Ok(new { success = true });
    }

    /// <summary>Pay-06: Admin hoàn tiền cho booking đã thanh toán (toàn phần hoặc một phần).</summary>
    [HttpPost("api/v1/admin/bookings/{bookingId:guid}/refund")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponse>> Refund(
        Guid bookingId,
        [FromBody] RefundPaymentRequest request,
        CancellationToken ct)
    {
        var result = await paymentService.RefundAsync(this.GetCurrentUserId(), bookingId, request, ct);
        return Ok(result);
    }
}
