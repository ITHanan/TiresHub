using ApplicationLayer.Features.OwnerCommunication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers;

[ApiController]
[Route("api/bookings/{bookingId:guid}")]
[Authorize]
public sealed class OwnerCommunicationController : ControllerBase
{
    private readonly OwnerCommunicationService _service;
    public OwnerCommunicationController(OwnerCommunicationService service) => _service = service;

    [HttpGet("contact-owner/preview")]
    public Task<IActionResult> Preview(Guid bookingId, CancellationToken ct) => Execute(() => _service.PreviewAsync(bookingId, ct));

    [HttpPost("contact-owner")]
    public Task<IActionResult> Send(Guid bookingId, [FromBody] SendOwnerMessageRequest request, CancellationToken ct)
        => Execute(() => _service.SendAsync(bookingId, request.Message, request.Channel, ct));

    [HttpGet("communication-logs")]
    public Task<IActionResult> Logs(Guid bookingId, CancellationToken ct) => Execute(() => _service.LogsAsync(bookingId, ct));

    [HttpGet("owner-decision")]
    public Task<IActionResult> GetDecision(Guid bookingId, CancellationToken ct) => Execute(() => _service.GetDecisionAsync(bookingId, ct));

    [HttpPost("owner-decision")]
    public Task<IActionResult> RecordDecision(Guid bookingId, [FromBody] RecordOwnerDecisionRequest request, CancellationToken ct)
        => Execute(() => _service.RecordDecisionAsync(bookingId, request.Decision, ct));

    private async Task<IActionResult> Execute<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (CommunicationException ex) { return StatusCode(ex.StatusCode, new { error = ex.Message, logId = ex.LogId }); }
    }
}

public sealed record SendOwnerMessageRequest(string? Message, string? Channel);
public sealed record RecordOwnerDecisionRequest(string? Decision);
