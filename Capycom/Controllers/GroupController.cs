using Capycom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.Common;

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
			CpcmGroup? group;
			try
			{
				if (!string.IsNullOrWhiteSpace(filter.NickName))
				{
					group = await _context.CpcmGroups.Where(g => g.CpcmGroupNickName == filter.NickName)
						.Include(g => g.CpcmGroupSubjectNavigation)
						.Include(g => g.CpcmGroupCityNavigation)
						.FirstOrDefaultAsync();
				}
				else
				{
					group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == filter.GroupId)
						.Include(g => g.CpcmGroupSubjectNavigation)
						.Include(g => g.CpcmGroupCityNavigation)
						.FirstOrDefaultAsync();
				}
				
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			if (group == null)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}




			List<CpcmPost> posts;
			try
			{
				posts = await _context.CpcmPosts.Where(c => c.CpcmGroupId == group.CpcmGroupId&& c.CpcmPostPublishedDate < DateTime.UtcNow).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			ICollection<CpcmPost> postsWithLikesCount = new List<CpcmPost>();
			GroupProfileAndPostsModel groupProfile = new();
			groupProfile.Group= group;
			foreach (var postik in posts)
			{
				postik.Group = group;
				postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
				long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
				long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
				postik.LikesCount = likes;
				postik.RepostsCount = reposts;
				postsWithLikesCount.Add(postik);
			}
			groupProfile.Posts = postsWithLikesCount;
			return View(groupProfile);
		}


		private async Task<CpcmPost?> GetFatherPostReccurent(CpcmPost cpcmPostFatherNavigation)
		{
			var father = await _context.CpcmPosts.Where(p => p.CpcmPostId == cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
			if (father != null)
			{
				father.CpcmPostFatherNavigation = await GetFatherPostReccurent(father);
				father.User = await _context.CpcmUsers.Where(p => p.CpcmUserId == father.CpcmUserId).FirstOrDefaultAsync();
				father.Group = await _context.CpcmGroups.Where(p => p.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
			}
			return father;
		}
	}
}
