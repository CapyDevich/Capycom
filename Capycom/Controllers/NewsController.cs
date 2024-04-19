using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Data.Common;
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
			Log.Information("Пользователь {UserId} открыл ленту новостей", HttpContext.User.FindFirstValue("CpcmUserId"));
			try
			{
				Guid userId = Guid.Parse(HttpContext.User.FindFirstValue("CpcmUserId"));
				List<PostModel> postsModel = new List<PostModel>();


				var groupIds = await _context.CpcmGroupfollowers
					.Where(gf => gf.CpcmUserId == userId)
					.Select(gf => gf.CpcmGroupId).ToListAsync();

				// Получаем всех друзей пользователя
				var friendIds = await _context.CpcmUserfriends
					.Where(f => f.CmcpUserId == userId && f.CpcmFriendRequestStatus==true || f.CmcpFriendId == userId && f.CpcmFriendRequestStatus == true)
					.Select(f => f.CmcpUserId == userId ? f.CmcpFriendId : f.CmcpUserId).ToListAsync();

				// Получаем всех пользователей, на которых подписан пользователь
				var followerIds = await _context.CpcmUserfollowers
					.Where(f => f.CpcmFollowerId == userId)
					.Select(f => f.CpcmUserId).ToListAsync();

				// Объединяем все ID в одну коллекцию
				var allUserIds = friendIds.Union(followerIds).ToList();

				// Получаем все посты этих групп и пользователей
				var posts = await _context.CpcmPosts
					.Where(p => (p.CpcmGroupId.HasValue && groupIds.Contains(p.CpcmGroupId.Value)) ||
								(p.CpcmUserId.HasValue && allUserIds.Contains(p.CpcmUserId.Value)))
					.Include(p => p.CpcmImages)
					//.Include(p => p.CpcmPostFatherNavigation)
					.OrderByDescending(c => c.CpcmPostPublishedDate).Where(p => p.CpcmIsDeleted == false)
					.Take(10)
					.ToListAsync();
				foreach (var post in posts)
				{
					var author = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
					if (author != null)
					{
						if (author.CpcmIsDeleted || author.CpcmUserBanned)
						{
							continue;
						}
					}
					else
					{
						var authorgGroup = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
						if (authorgGroup != null && authorgGroup.CpcmIsDeleted || authorgGroup.CpcmGroupBanned)
						{
							continue;
						}
					}

					if (post.CpcmPostFather != null)
					{
						post.CpcmPostFatherNavigation = await GetFatherPostReccurent(post);
					}
					CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
					CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
					long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();
					long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();
					if (User.Identity.IsAuthenticated)
					{
						var authUserId = GetUserIdString();
						var authFollower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == authUserId && f.CpcmGroupId == groupOwner.CpcmGroupId).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
						groupOwner.UserFollowerRole = authFollower.CpcmUserRoleNavigation;
						post.Group.UserFollowerRole = authFollower.CpcmUserRoleNavigation;
					}
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								post.CpcmPostPublishedDate-= offset;

							}
						}
					}

					PostModel postModel = new() { Post = post, UserOwner = userOwner, GroupOwner = groupOwner, LikesCount = likes, RepostsCount = reposts };
					postsModel.Add(postModel);
				}

				return View(postsModel);
			}
			catch(DbUpdateException ex)
			{
				Log.Error(ex, "Произошла ошибка при обращении к бд. Не удалось выполнить запрос. {user}", HttpContext.User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Произошла ошибка при обращении к бд. Не удалось выполнить запрос. {user}", HttpContext.User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
		}
		[Authorize]
		public async Task<IActionResult> GetNextPosts(Guid lastPostId)
		{
			Log.Information("Пользователь {UserId} запросил следующие посты", HttpContext.User.FindFirstValue("CpcmUserId"));	
			try
			{
				Guid userId = Guid.Parse(HttpContext.User.FindFirstValue("CpcmUserId"));
				List<PostModel> postsModel = new List<PostModel>();
				var lastPost = await _context.CpcmPosts.Where(p => p.CpcmPostId == lastPostId).FirstOrDefaultAsync();
				if (lastPost == null)
				{
					Log.Warning("Пользователь {UserId} запросил следующие посты, но последний пост не найден", HttpContext.User.FindFirstValue("CpcmUserId"));
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
					.OrderByDescending(c => c.CpcmPostPublishedDate).Where(p => p.CpcmIsDeleted == false)
					.Take(10)
					.ToListAsync();
				foreach (var post in posts)
				{
					var author = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
					if (author != null)
					{
						if(author.CpcmIsDeleted || author.CpcmUserBanned)
						{
							continue;
						}
					}
					else
					{
						var authorgGroup = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
						if(authorgGroup!=null && authorgGroup.CpcmIsDeleted || authorgGroup.CpcmGroupBanned)
						{
							continue;
						}
					}

					if (post.CpcmPostFather != null)
					{
						post.CpcmPostFatherNavigation = await GetFatherPostReccurent(post);
					}
					CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
					CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
					long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();
					long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								post.CpcmPostPublishedDate -= offset;

							}
						}
					}
					PostModel postModel = new() { Post = post, UserOwner = userOwner, GroupOwner = groupOwner, LikesCount = likes, RepostsCount = reposts };
					postsModel.Add(postModel);
				}

				return PartialView(postsModel);
			}
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Произошла ошибка при обращении к бд. Не удалось выполнить запрос. {user}", HttpContext.User.FindFirstValue("CpcmUserId"));
				return StatusCode(500);
			}
			catch (DbException ex)
			{
				Log.Error(ex,"Произошла ошибка при обращении к бд. Не удалось выполнить запрос. {user}", HttpContext.User.FindFirstValue("CpcmUserId"));
				return StatusCode(500);
			}
		}

		private async Task<CpcmPost?> GetFatherPostReccurent(CpcmPost cpcmPostFatherNavigation)
		{
			try
			{
				var father = await _context.CpcmPosts.Where(p => p.CpcmPostId == cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
				if (father != null)
				{
					father.CpcmPostFatherNavigation = await GetFatherPostReccurent(father);
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								father.CpcmPostPublishedDate -= offset;

							}
						}
					}
					var author = await _context.CpcmUsers.Where(u => u.CpcmUserId == father.CpcmUserId).FirstOrDefaultAsync();
					if(author == null)
					{
						var group = await _context.CpcmGroups.Where(u => u.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
						father.Group= group;
					}
					else
					{
						father.User = author;
					}
				}
				return father;
			}
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Не удалось выгрузить родительские посты {@fathrepostnavigation}", cpcmPostFatherNavigation);
				throw;
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Не удалось выгрузить родительские посты {@fathrepostnavigation}", cpcmPostFatherNavigation);
				throw;
			}
		}
		private Guid GetUserIdString()
		{
			if (User.Identity.IsAuthenticated)
			{
				return Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
			}
			throw new InvalidOperationException("User is not authenticated");
		}
	}
}
