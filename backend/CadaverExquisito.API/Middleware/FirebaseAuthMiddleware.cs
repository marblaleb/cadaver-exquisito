using FirebaseAdmin.Auth;
using Microsoft.Extensions.Hosting;

namespace CadaverExquisito.API.Middleware;

public class FirebaseAuthMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Dev bypass: accept X-Dev-User-Id header when no real token is present
        if (env.IsDevelopment())
        {
            var devUserId = context.Request.Headers["X-Dev-User-Id"].ToString();
            if (!string.IsNullOrEmpty(devUserId))
            {
                context.Items["UserId"] = devUserId;
                context.Items["Email"] = "dev@local.test";
                await next(context);
                return;
            }
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];
            try
            {
                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                context.Items["UserId"] = decoded.Uid;
                context.Items["Email"] = decoded.Claims.GetValueOrDefault("email")?.ToString() ?? string.Empty;
            }
            catch
            {
                // Invalid token — controllers enforce auth by calling GetUserId()
            }
        }
        await next(context);
    }
}

public static class HttpContextExtensions
{
    public static string GetUserId(this HttpContext context)
    {
        var userId = context.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Not authenticated.");
        return userId;
    }
}
