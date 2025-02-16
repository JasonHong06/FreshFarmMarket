using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using FreshFarmMarket.Model;
using FreshFarmMarket.Helpers;

namespace FreshFarmMarket.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private const int SessionTimeoutMinutes = 30;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)

            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    string? lastActivity = context.Session.GetString("LastActivity");

                    if (!string.IsNullOrEmpty(lastActivity))
                    {
                        DateTime lastActivityTime = DateTime.Parse(lastActivity);

                        // ✅ Auto logout after inactivity
                        if (DateTime.UtcNow.Subtract(lastActivityTime).TotalMinutes > SessionTimeoutMinutes)
                        {
                            user.CurrentSessionId = string.Empty; // ✅ Use empty string instead of null

                            await userManager.UpdateAsync(user);

                            await signInManager.SignOutAsync();
                            context.Session.Clear();
                            context.Response.Redirect("/Logout");
                            return;
                        }
                    }

                    // ✅ Update last activity timestamp
                    context.Session.SetString("LastActivity", DateTime.UtcNow.ToString());

                    // ✅ Prevent multiple logins
                    string? sessionId = context.Session.GetString("UserSessionId");

                    if (sessionId == null || user.CurrentSessionId != sessionId)
                    {
                        await signInManager.SignOutAsync();
                        context.Session.Clear();
                        context.Response.Redirect("/Login");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
