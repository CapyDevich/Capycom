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
				//.Include(p => p.CpcmPostFatherNavigation)
				.OrderByDescending(c => c.CpcmPostPublishedDate)
				.Take(10)
				.ToListAsync();
			foreach (var post in posts)
			{
				if(post.CpcmPostFather != null)
				{
					post.CpcmPostFatherNavigation = await GetFatherPostReccurent(post);
				}
				CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
				CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
				long likes = await _context.Database.ExecuteSqlInterpolatedAsync($@"SELECT COUNT(*) FROM CPCM_POSTLIKES WHERE CPCM_PostID = '{post.CpcmGroupId}'");
				long reposts = await _context.Database.ExecuteSqlInterpolatedAsync($@"SELECT COUNT(*) FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = '{post.CpcmGroupId}'");

				PostModel postModel = new() { Post = post, UserOwner = userOwner, GroupOwner = groupOwner, LikesCount = likes, RepostsCount = reposts};
				postsModel.Add(postModel);
			}

			return View(postsModel);
		}
		[Authorize]
		public async Task<IActionResult> GetNextPosts(Guid lastPostId)
		{
			Guid userId = Guid.Parse(HttpContext.User.FindFirstValue("CpcmUserId"));
			List<PostModel> postsModel = new List<PostModel>();
			var lastPost = await _context.CpcmPosts.Where(p => p.CpcmPostId == lastPostId).FirstOrDefaultAsync();
			if (lastPost == null)
			{
				return StatusCode(404);
			}

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
				.Where(p => ((p.CpcmGroupId.HasValue && groupIds.Contains(p.CpcmGroupId.Value)) ||
							(p.CpcmUserId.HasValue && allUserIds.Contains(p.CpcmUserId.Value))) &&
							p.CpcmPostPublishedDate < lastPost.CpcmPostPublishedDate)
				.Include(p => p.CpcmImages)
				//.Include(p => p.CpcmPostFatherNavigation)
				.OrderByDescending(c => c.CpcmPostPublishedDate)
				.Take(10)
				.ToListAsync();
			foreach (var post in posts)
			{
				if (post.CpcmPostFather != null)
				{
					post.CpcmPostFatherNavigation = await GetFatherPostReccurent(post);
				}
				CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
				CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
				long likes = await _context.Database.ExecuteSqlInterpolatedAsync($@"SELECT COUNT(*) FROM CPCM_POSTLIKES WHERE CPCM_PostID = '{post.CpcmGroupId}'");
				long reposts = await _context.Database.ExecuteSqlInterpolatedAsync($@"SELECT COUNT(*) FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = '{post.CpcmGroupId}'");

				PostModel postModel = new() { Post = post, UserOwner = userOwner, GroupOwner = groupOwner, LikesCount = likes, RepostsCount = reposts };
				postsModel.Add(postModel);
			}

			return Json(postsModel);
		}

		private async Task<CpcmPost?> GetFatherPostReccurent(CpcmPost cpcmPostFatherNavigation)
		{
			var father = await _context.CpcmPosts.Where(p => p.CpcmPostId == cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
			if (father != null)
			{
				father.CpcmPostFatherNavigation = await GetFatherPostReccurent(father);
			}
			return father;
		}
	}
}
