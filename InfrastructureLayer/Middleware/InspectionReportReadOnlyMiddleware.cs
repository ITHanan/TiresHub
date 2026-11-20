using System.Security.Claims;
using ApplicationLayer.Interfaces;
using DomainLayer.Auditing;
using Microsoft.AspNetCore.Http;

namespace InfrastructureLayer.Middleware;

public sealed class InspectionReportReadOnlyMiddleware
{
    private readonly RequestDelegate _next;

    public InspectionReportReadOnlyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAuditRepository audit)
    {
        var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var isReportPath = segments is { Length: >= 4 }
            && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)
            && segments[1].Equals("bookings", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(segments[2], out _)
            && segments[3].Equals("inspection-report", StringComparison.OrdinalIgnoreCase);

        if (isReportPath && (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method)))
        {
            Guid.TryParse(segments![2], out var bookingId);
            Guid? userId = Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId : null;
            await audit.LogAsync(userId, AuditActions.InspectionReportModificationAttempt, "Booking", bookingId,
                false, "Inspection reports are read-only.", context.Request.Method);
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            context.Response.Headers.Allow = "GET";
            await context.Response.WriteAsJsonAsync(new { error = "Inspection reports are read-only." });
            return;
        }

        await _next(context);
    }
}
