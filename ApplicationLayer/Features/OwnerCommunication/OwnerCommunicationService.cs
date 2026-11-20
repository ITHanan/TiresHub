using System.Text.Json;
using System.Net.Mail;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Identity;
using DomainLayer.Bookings;
using DomainLayer.Communication;
using DomainLayer.Enums;
using DomainLayer.Users;

namespace ApplicationLayer.Features.OwnerCommunication;

public interface IOwnerCommunicationRepository
{
    Task<Booking?> GetBookingAsync(Guid id, CancellationToken ct);
    Task<User?> GetOwnerAsync(Guid vehicleId, CancellationToken ct);
    Task<InspectionReport?> GetReportAsync(Guid bookingId, CancellationToken ct);
    Task AddLogAsync(CommunicationLog log, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task<IReadOnlyList<CommunicationLog>> GetLogsAsync(Guid bookingId, CancellationToken ct);
}

public interface IOwnerMessageSender
{
    Task SendEmailAsync(string recipient, string message, IReadOnlyList<string> evidenceUrls, CancellationToken ct);
}

public sealed class CommunicationException : Exception
{
    public int StatusCode { get; }
    public Guid? LogId { get; }
    public CommunicationException(int statusCode, string message, Guid? logId = null) : base(message)
        => (StatusCode, LogId) = (statusCode, logId);
}

public sealed record CommunicationPreview(string Channel, string Message, IReadOnlyList<string> Photos, bool CanSend, string? Reason);
public sealed record CommunicationLogDto(Guid Id, string Channel, string Status, DateTime Timestamp, string? Content, IReadOnlyList<string> EvidenceRefs);
public sealed record OwnerDecisionDto(string Status, DateTime? Timestamp, string? Channel);

public sealed class OwnerCommunicationService
{
    private readonly IOwnerCommunicationRepository _repo;
    private readonly IOwnerMessageSender _sender;
    private readonly ICurrentUser _user;
    private readonly IAuditRepository _audit;

    public OwnerCommunicationService(IOwnerCommunicationRepository repo, IOwnerMessageSender sender, ICurrentUser user, IAuditRepository audit)
        => (_repo, _sender, _user, _audit) = (repo, sender, user, audit);

    private async Task<Booking> ManagerBooking(Guid bookingId, CancellationToken ct)
    {
        if (!_user.IsAuthenticated) throw new CommunicationException(401, "Authentication required.");
        if (_user.Role is not (UserRole.ShopManager or UserRole.ShopOwner))
        {
            await Denied(bookingId);
            throw new CommunicationException(403, "Access denied.");
        }
        var booking = await _repo.GetBookingAsync(bookingId, ct) ?? throw new CommunicationException(404, "Booking not found.");
        if (_user.Role == UserRole.ShopManager && _user.BranchId != booking.BranchId)
        {
            await Denied(bookingId);
            throw new CommunicationException(403, "Access denied.");
        }
        return booking;
    }

    private Task Denied(Guid bookingId) => _audit.LogAsync(_user.UserId, "UnauthorizedOwnerCommunication", "Booking", bookingId, false, "Role or branch access denied.");

    public async Task<CommunicationPreview> PreviewAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await ManagerBooking(bookingId, ct);
        var owner = await _repo.GetOwnerAsync(booking.VehicleId, ct);
        var report = await _repo.GetReportAsync(bookingId, ct);
        var photos = report?.Photos.Select(p => p.ImageUrl).ToArray() ?? Array.Empty<string>();
        var reason = owner == null || !IsValidEmail(owner.UserEmail) ? "Owner email is missing or invalid."
            : report == null ? "Inspection report not found."
            : photos.Length == 0 ? "Inspection report has no photos." : null;
        return new("Email", "Please review the inspection evidence and approve or reject the recommended maintenance.", photos, reason == null, reason);
    }

    public async Task<CommunicationLogDto> SendAsync(Guid bookingId, string? message, string? channel, CancellationToken ct)
    {
        var booking = await ManagerBooking(bookingId, ct);
        await _audit.LogAsync(_user.UserId, "ContactOwnerAttempt", "Booking", bookingId, true);
        if (!string.Equals(channel, "Email", StringComparison.OrdinalIgnoreCase))
            throw new CommunicationException(400, "Only Email is supported.");
        if (string.IsNullOrWhiteSpace(message) || message.Length > 4000)
            throw new CommunicationException(400, "Message is required and must be at most 4000 characters.");
        var owner = await _repo.GetOwnerAsync(booking.VehicleId, ct);
        if (owner == null || !IsValidEmail(owner.UserEmail))
            throw new CommunicationException(400, "Owner email is missing or invalid.");
        var report = await _repo.GetReportAsync(bookingId, ct);
        if (report == null) throw new CommunicationException(400, "Inspection report not found.");
        var photos = report.Photos.Select(p => p.ImageUrl).ToArray();
        if (photos.Length == 0) throw new CommunicationException(400, "Inspection report has no photos.");

        var log = new CommunicationLog(bookingId, owner.Id, owner.UserEmail, CommunicationChannel.Email, message.Trim(), JsonSerializer.Serialize(photos));
        await _repo.AddLogAsync(log, ct);
        await _repo.SaveAsync(ct);
        try
        {
            await _sender.SendEmailAsync(owner.UserEmail, message.Trim(), photos, ct);
        }
        catch (Exception)
        {
            log.Complete(false);
            await _repo.SaveAsync(CancellationToken.None);
            await _audit.LogAsync(_user.UserId, "ContactOwnerSendFailed", "Booking", bookingId, false, "Email delivery failed.", log.Id.ToString());
            throw new CommunicationException(502, "Message could not be sent.", log.Id);
        }
        log.Complete(true);
        await _repo.SaveAsync(CancellationToken.None);
        await _audit.LogAsync(_user.UserId, "ContactOwnerSent", "Booking", bookingId, true, null, log.Id.ToString());
        return ToDto(log);
    }

    public async Task<IReadOnlyList<CommunicationLogDto>> LogsAsync(Guid bookingId, CancellationToken ct)
    {
        await ManagerBooking(bookingId, ct);
        return (await _repo.GetLogsAsync(bookingId, ct)).Select(ToDto).ToArray();
    }

    public async Task<OwnerDecisionDto> GetDecisionAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await DecisionBooking(bookingId, ct);
        return Decision(booking);
    }

    public async Task<OwnerDecisionDto> RecordDecisionAsync(Guid bookingId, string? decision, CancellationToken ct)
    {
        if (!_user.IsAuthenticated) throw new CommunicationException(401, "Authentication required.");
        var booking = await _repo.GetBookingAsync(bookingId, ct) ?? throw new CommunicationException(404, "Booking not found.");
        var owner = await _repo.GetOwnerAsync(booking.VehicleId, ct);
        if (_user.Role != UserRole.VehicleOwner || owner == null || owner.Id != _user.UserId)
        {
            await Denied(bookingId);
            throw new CommunicationException(403, "Only the booking's vehicle owner can submit a decision.");
        }
        if (decision != "Approved" && decision != "Rejected")
            throw new CommunicationException(400, "Decision must be Approved or Rejected.");
        booking.RecordOwnerDecision(Enum.Parse<OwnerDecisionType>(decision), "App");
        await _repo.SaveAsync(ct);
        await _audit.LogAsync(_user.UserId, "OwnerDecisionRecorded", "Booking", bookingId, true, null, decision);
        return Decision(booking);
    }

    private async Task<Booking> DecisionBooking(Guid id, CancellationToken ct)
    {
        if (!_user.IsAuthenticated) throw new CommunicationException(401, "Authentication required.");
        if (_user.Role != UserRole.VehicleOwner) return await ManagerBooking(id, ct);
        var booking = await _repo.GetBookingAsync(id, ct) ?? throw new CommunicationException(404, "Booking not found.");
        var owner = await _repo.GetOwnerAsync(booking.VehicleId, ct);
        if (owner?.Id != _user.UserId) { await Denied(id); throw new CommunicationException(403, "Access denied."); }
        return booking;
    }

    private static OwnerDecisionDto Decision(Booking b) => new(b.OwnerDecision?.ToString() ?? "Pending", b.OwnerDecisionAt, b.OwnerDecisionChannel);
    private static bool IsValidEmail(string? value) => !string.IsNullOrWhiteSpace(value)
        && !value.Contains('\r') && !value.Contains('\n')
        && MailAddress.TryCreate(value, out var address)
        && string.Equals(value, address.Address, StringComparison.OrdinalIgnoreCase);
    private static CommunicationLogDto ToDto(CommunicationLog log) => new(log.Id, log.Channel.ToString(), log.Status, log.CreatedAt, log.Content,
        log.EvidenceRefs == null ? Array.Empty<string>() : JsonSerializer.Deserialize<string[]>(log.EvidenceRefs) ?? Array.Empty<string>());
}
