using ApplicationLayer.Features.Bookings.DTOs;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Common;
using MediatR;
using System.Linq;
using DomainLayer.Enums;
using DomainLayer.Auditing;
using ApplicationLayer.Interfaces;

namespace ApplicationLayer.Features.Bookings.Queries.GetInspectionReportByBooking;

public class GetInspectionReportByBookingQueryHandler : IRequestHandler<GetInspectionReportByBookingQuery, OperationResult<GetInspectionReportByBookingResponse>>
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ApplicationLayer.Interfaces.Identity.ICurrentUser _currentUser;
    private readonly IAuditRepository _audit;

    public GetInspectionReportByBookingQueryHandler(IBookingRepository bookingRepo, ApplicationLayer.Interfaces.Identity.ICurrentUser currentUser, IAuditRepository audit)
    {
        _bookingRepo = bookingRepo;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<OperationResult<GetInspectionReportByBookingResponse>> Handle(GetInspectionReportByBookingQuery request, CancellationToken cancellationToken)
    {
        // Validation is handled by FluentValidation pipeline

        // Authorization rules
        if (!_currentUser.IsAuthenticated)
            return OperationResult<GetInspectionReportByBookingResponse>.Failure("User is not authenticated.");

        if (_currentUser.Role != UserRole.ShopManager && _currentUser.Role != UserRole.ShopOwner)
        {
            await _audit.LogAsync(_currentUser.UserId, AuditActions.UnauthorizedInspectionReportAccess,
                "Booking", request.BookingId, false, "User role cannot view inspection reports.");
            return OperationResult<GetInspectionReportByBookingResponse>.Failure("Access denied.");
        }

        // Fetch booking first to perform branch checks
        var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            return OperationResult<GetInspectionReportByBookingResponse>.Failure("Booking not found.");
        }

        // Role enforcement: ShopManager limited to their branch; ShopOwner treated as admin/bypass
        if (_currentUser.Role == UserRole.ShopManager)
        {
            if (!_currentUser.BranchId.HasValue || _currentUser.BranchId.Value != booking.BranchId)
            {
                await _audit.LogAsync(_currentUser.UserId, AuditActions.UnauthorizedInspectionReportAccess,
                    "Booking", request.BookingId, false, "Shop manager is not assigned to the booking branch.");
                return OperationResult<GetInspectionReportByBookingResponse>.Failure("Shop manager does not have access to this booking.");
            }
        }

        var bookingSummary = new BookingSummaryDto
        {
            BookingId = booking.Id,
            AppointmentDate = booking.AppointmentDate,
            CustomerName = await _bookingRepo.GetVehicleOwnerNameAsync(booking.VehicleId, cancellationToken) ?? string.Empty,
            ServiceType = booking.ServiceType
        };

        // Try load report via repository (IBookingRepository can expose a method)
        var report = await _bookingRepo.GetInspectionReportByBookingIdAsync(booking.Id, cancellationToken);

        InspectionReportDto? reportDto = null;

        if (report != null)
        {
            reportDto = new InspectionReportDto
            {
                Notes = report.Notes,
                Photos = report.Photos.Select(p => p.ImageUrl).ToList(),
                CreatedAt = report.CreatedAt,
                CreatedByName = report.CreatedByUser?.Name ?? "Unknown"
            };
        }

        // owner decision lookup (if any) - assume repository method exists
        var ownerDecision = await _bookingRepo.GetOwnerDecisionByBookingIdAsync(booking.Id, cancellationToken);

        var final = new GetInspectionReportByBookingResponse
        {
            Booking = bookingSummary,
            Report = reportDto,
            OwnerDecision = ownerDecision
        };

        return OperationResult<GetInspectionReportByBookingResponse>.Success(final);
    }
}
