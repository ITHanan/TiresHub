using ApplicationLayer.Features.Bookings.DTOs;
using ApplicationLayer.Interfaces.Bookings;
using DomainLayer.Common;
using MediatR;
using System.Linq;
using DomainLayer.Enums;

namespace ApplicationLayer.Features.Bookings.Queries.GetInspectionReportByBooking;

public class GetInspectionReportByBookingQueryHandler : IRequestHandler<GetInspectionReportByBookingQuery, OperationResult<GetInspectionReportByBookingResponse>>
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ApplicationLayer.Interfaces.Identity.ICurrentUser _currentUser;

    public GetInspectionReportByBookingQueryHandler(IBookingRepository bookingRepo, ApplicationLayer.Interfaces.Identity.ICurrentUser currentUser)
    {
        _bookingRepo = bookingRepo;
        _currentUser = currentUser;
    }

    public async Task<OperationResult<GetInspectionReportByBookingResponse>> Handle(GetInspectionReportByBookingQuery request, CancellationToken cancellationToken)
    {
        // Validation is handled by FluentValidation pipeline

        // Authorization rules
        if (!_currentUser.IsAuthenticated)
            return OperationResult<GetInspectionReportByBookingResponse>.Failure("User is not authenticated.");

        // Fetch booking first to perform branch checks
        var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            // Return a successful result with null report (A1) but BookingId empty to indicate missing booking
            var response = new GetInspectionReportByBookingResponse
            {
                Booking = new BookingSummaryDto
                {
                    BookingId = Guid.Empty,
                    AppointmentDate = DateTime.MinValue,
                    CustomerName = string.Empty,
                    ServiceType = default
                },
                Report = null,
                OwnerDecision = null
            };

            return OperationResult<GetInspectionReportByBookingResponse>.Success(response);
        }

        // Role enforcement: ShopManager limited to their branch; ShopOwner treated as admin/bypass
        if (_currentUser.Role == UserRole.ShopManager)
        {
            if (!_currentUser.BranchId.HasValue || _currentUser.BranchId.Value != booking.BranchId)
                return OperationResult<GetInspectionReportByBookingResponse>.Failure("Shop manager does not have access to this booking.");
        }
        else
        {
            if (_currentUser.Role != UserRole.ShopOwner)
            {
                return OperationResult<GetInspectionReportByBookingResponse>.Failure("Only shop managers or admins can view inspection reports.");
            }
        }

        var bookingSummary = new BookingSummaryDto
        {
            BookingId = booking.Id,
            AppointmentDate = booking.AppointmentDate,
            CustomerName = "",
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
