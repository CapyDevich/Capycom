using Capycom.Models;
using Microsoft.AspNetCore.Mvc;

namespace Capycom.Controllers
{
    public class ErrorController : Controller
	{

		public async Task<IActionResult> Error(ErrorModel errorModel)
		{
			if (errorModel != null)
			{
				Response.StatusCode = errorModel.StatusCode;
				return View("UserError",errorModel);
			}
			Response.StatusCode = 500;
			return View("UserError",errorModel);
		}

		public async Task<IActionResult> Code403()
		{
			return StatusCode(403);
		}

	}
}
