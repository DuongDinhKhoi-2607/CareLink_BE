using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Bookings;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    /// <summary>Book-01: customer creates a booking (status PendingPayment).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingService.CreateAsync(this.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>List the current user's bookings (customer: own, nurse: assigned, admin: all).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetList(
        [FromQuery] BookingQueryRequest query,
        CancellationToken cancellationToken)
    {
        var result = await bookingService.GetListAsync(this.GetCurrentUserId(), query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Book-02: Xem chi tiết ca chăm sóc theo ID (Khách hàng, Điều dưỡng hoặc Admin).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.GetByIdAsync(this.GetCurrentUserId(), id, cancellationToken);
        return Ok(booking);
    }

    /// <summary>Book-03a: nurse accepts the booking (PendingAcceptance -> Accepted).</summary>
    [HttpPut("{id:guid}/accept")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Accept(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.AcceptAsync(this.GetCurrentUserId(), id, cancellationToken);
        return Ok(booking);
    }

    /// <summary>Book-03b: nurse rejects the booking; slot is released and the customer is notified.</summary>
    [HttpPut("{id:guid}/reject")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Reject(
        Guid id,
        [FromBody] RejectBookingRequest? request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingService.RejectAsync(
            this.GetCurrentUserId(), id, request ?? new RejectBookingRequest(), cancellationToken);
        return Ok(booking);
    }

    /// <summary>Book-04: nurse checks in and starts the visit (Accepted -> InProgress).</summary>
    [HttpPut("{id:guid}/start")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingResponse>> Start(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.StartAsync(this.GetCurrentUserId(), id, cancellationToken);
        return Ok(booking);
    }

    /// <summary>Book-05: nurse finishes the visit after submitting the health record (InProgress -> Completed).</summary>
    [HttpPut("{id:guid}/finish")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingResponse>> Finish(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.FinishAsync(this.GetCurrentUserId(), id, cancellationToken);
        return Ok(booking);
    }

    /// <summary>Book-06: customer or nurse cancels the booking; refund depends on the cancellation policy.</summary>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Cancel(
        Guid id,
        [FromBody] CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingService.CancelAsync(this.GetCurrentUserId(), id, request, cancellationToken);
        return Ok(booking);
    }
}
