using FluentValidation;

namespace ApplicationLayer.Features.Bookings.Queries.GetInspectionReportByBooking;

public class GetInspectionReportValidator : AbstractValidator<GetInspectionReportByBookingQuery>
{
    public GetInspectionReportValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("BookingId is required.");
    }
}
