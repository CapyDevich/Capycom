using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Capycom
{
    public class UserAuthMiddleware
    {
		private readonly RequestDelegate _next;
		private readonly CapycomContext _context;

		public UserAuthMiddleware(RequestDelegate next, CapycomContext context)
		{
			_next = next;
			_context = context;
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.User.Identity.IsAuthenticated)
			{
				var user = _context.CpcmUsers.FirstOrDefault(u => u.CpcmUserId.ToString() == context.User.FindFirstValue("CpcmUserId"));
				if (user != null && user.CpcmUserBanned || user.CpcmIsDeleted)
				{
					await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
					context.Response.Redirect("/UserLogIn/Index");
					return;
				}
			}

			await _next(context);
		}
	}
}
