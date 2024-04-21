using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static System.Formats.Asn1.AsnWriter;

namespace Capycom
{
	public class AuthFilter : IAuthorizationFilter
	{
		private readonly CapycomContext _dbContext;

		public AuthFilter(CapycomContext dbContext)
		{
			_dbContext = dbContext;
		}
		public void OnAuthorization(AuthorizationFilterContext context)
		{

			if (context.HttpContext.User !=null && context.HttpContext.User.Identity.IsAuthenticated)
			{
				var user = _dbContext.CpcmUsers.FirstOrDefault(u => u.CpcmUserId.ToString() == context.HttpContext.User.FindFirst("CpcmUserId").Value);
				if (user != null && user.CpcmUserBanned || user.CpcmIsDeleted)
				{
					context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
					context.Result = new RedirectToActionResult("Index", "UserLogIn",null);
					return;
				}
			}

		}

	}
}
