using DomainLayer.Bookings;

namespace ApplicationLayer.Features.Bookings.Validators
{
    /// <summary>
    /// Validation helper for booking operations that require employee assignment.
    /// UC-15 (BE-70): Assignment Required Rule
    /// </summary>
    public static class BookingAssignmentValidator
    {
        /// <summary>
        /// Validates that a booking has an assigned employee before allowing inspection or preparation tasks.
        /// </summary>
        /// <param name="booking">The booking to validate</param>
        /// <returns>True if booking has an assigned employee, false otherwise</returns>
        public static bool HasAssignedEmployee(Booking booking)
        {
            return booking.AssignedEmployeeId.HasValue;
        }

        /// <summary>
        /// Gets the error message for missing employee assignment.
        /// </summary>
        public static string GetMissingAssignmentErrorMessage()
        {
            return "An employee must be assigned to the booking before proceeding with inspection or preparation tasks.";
        }
    }
}
