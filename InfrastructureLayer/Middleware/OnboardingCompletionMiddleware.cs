using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Middleware
{
    public class OnboardingCompletionMiddleware
    {
        private readonly RequestDelegate _next;

        public OnboardingCompletionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLower();
            if (path != null && path.StartsWith("/api/auth"))
            {
                await _next(context);
                return;
            }

            var onbordingClaim = context.User.FindFirst("OnboardingCompleted");

            var onboardingCompleted = onbordingClaim != null &&
                                      bool.TryParse(onbordingClaim.Value, out var completed) &&
                                      completed;

            if (!onboardingCompleted) 
            {

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var responseMessage = new
                {
                    error= "onboardingNotCompleted ",
                    message = "Please complete Onboarding before accessing the system."
                };
                await context.Response.WriteAsync("{\"message\": \"Onboarding not completed. Access denied.\"}");
                return;
            }

            await _next(context);   
          
        }
    }
}
