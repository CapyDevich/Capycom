using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Capycom
{
    public class UserAuthMiddleware
    {
		private readonly RequestDelegate _next;
		private CapycomContext _context;
		private readonly IServiceProvider _serviceProvider;

		public UserAuthMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
		{
			_next = next;
			_serviceProvider = serviceProvider;
		}

		public async Task Invoke(HttpContext context)
		{
			using (var scope = _serviceProvider.CreateAsyncScope())
			{
				_context = scope.ServiceProvider.GetRequiredService<CapycomContext>();
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
			}

			await _next(context);
		}
	}
}
