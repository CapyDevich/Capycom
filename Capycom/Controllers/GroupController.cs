using Capycom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Capycom.Controllers
{
	public class GroupController : Controller
	{
		private readonly CapycomContext _context;
		private readonly MyConfig _config;
		private readonly ILogger<GroupController> _logger;

		public GroupController(ILogger<GroupController> logger, CapycomContext context, IOptions<MyConfig> config)
		{
			_context = context;
			_config = config.Value;
			_logger = logger;
		}
		public async Task<IActionResult> Index(GroupFilterModel filter)
		{
			return View();
		}
	}
}
