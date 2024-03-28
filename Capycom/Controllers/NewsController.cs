using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Capycom.Controllers
{
	public class NewsController : Controller
	{
		private readonly CapycomContext _context;
		private readonly MyConfig _config;
		private readonly ILogger<NewsController> _logger;

		public NewsController(ILogger<NewsController> logger, CapycomContext context, IOptions<MyConfig> config)
		{
			_context = context;
			_config = config.Value;
			_logger = logger;
		}

		[Authorize]
		public async Task<IActionResult> Index()
		{
			Guid userId = Guid.Parse(HttpContext.User.FindFirstValue("CpcmUserId"));
			List<PostModel> postsModel = new List<PostModel>();


			var groupIds = await _context.CpcmGroupfollowers
				.Where(gf => gf.CpcmUserId == userId)
				.Select(gf => gf.CpcmGroupId).ToListAsync();

			// Получаем всех друзей пользователя
			var friendIds = await _context.CpcmUserfriends
				.Where(f => f.CmcpUserId == userId || f.CmcpFriendId == userId)
				.Select(f => f.CmcpUserId == userId ? f.CmcpFriendId : f.CmcpUserId).ToListAsync();

			// Получаем всех пользователей, на которых подписан пользователь
			var followerIds = await _context.CpcmUserfollowers
				.Where(f => f.CpcmUserId == userId)
				.Select(f => f.CpcmFollowersId).ToListAsync();

			// Объединяем все ID в одну коллекцию
			var allUserIds = friendIds.Union(followerIds).ToList();

			// Получаем все посты этих групп и пользователей
			var posts = await _context.CpcmPosts
				.Where(p => (p.CpcmGroupId.HasValue && groupIds.Contains(p.CpcmGroupId.Value)) ||
							(p.CpcmUserId.HasValue && allUserIds.Contains(p.CpcmUserId.Value)))
				.Include(p => p.CpcmImages)
				.Include(p => p.CpcmPostFatherNavigation)
				.ToListAsync();
			foreach (var post in posts)
			{
				if(post.CpcmPostFatherNavigation != null)
				{
					post.CpcmPostFatherNavigation = 
				}
			}

			return View();
		}
	}
}
