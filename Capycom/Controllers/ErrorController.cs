using Capycom.Models;
using Microsoft.AspNetCore.Mvc;

namespace Capycom.Controllers
{
    public class ErrorController : Controller
	{

		public IActionResult Error(ErrorModel errorModel)
		{
			if (errorModel != null)
			{
				Response.StatusCode = errorModel.StatusCode;
				return View("UserError",errorModel);
			}
			Response.StatusCode = 500;
			return View("UserError",errorModel);
		}
	}
}
