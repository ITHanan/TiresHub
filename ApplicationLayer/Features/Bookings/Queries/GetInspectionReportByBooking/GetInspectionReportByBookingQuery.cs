using ApplicationLayer.Features.Bookings.DTOs;
using DomainLayer.Common;
using MediatR;

namespace ApplicationLayer.Features.Bookings.Queries.GetInspectionReportByBooking;

public record GetInspectionReportByBookingQuery(Guid BookingId)
    : IRequest<OperationResult<GetInspectionReportByBookingResponse>>;
