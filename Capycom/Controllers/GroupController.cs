using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Reflection;
using System.Security.Claims;
using static NuGet.Packaging.PackagingConstants;

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
			if (group.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}




			List<CpcmPost> posts;
			long liked;
			CpcmGroupfollower follower = null;
			try
			{
				posts = await _context.CpcmPosts.Where(c => c.CpcmGroupId == group.CpcmGroupId && c.CpcmPostPublishedDate < DateTime.UtcNow).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
				if (User.Identity.IsAuthenticated)
				{
					follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmGroupId == group.CpcmGroupId && f.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId")).FirstOrDefaultAsync(); 
				}
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
			groupProfile.Group = group;
			foreach (var postik in posts)
			{
				postik.Group = group;
				postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
				long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
				long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
				postik.LikesCount = likes;
				postik.RepostsCount = reposts;

				if (User.Identity.IsAuthenticated)
				{
					try
					{
						liked = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId} && CPCM_UserID = {postik.CpcmUserId}").CountAsync();
					}
					catch (DbException)
					{
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
						return View("UserError");
					}
					if (liked > 0)
						postik.IsLiked = true;
					else
						postik.IsLiked = false;

					if (follower != null)
					{
						group.IsFollowed = true;
						if(follower.CpcmUserRole==CpcmGroupfollower.AuthorRole || follower.CpcmUserRole == CpcmGroupfollower.AdminRole)
						{
							group.IsAdmin = true;
						}
					}

				}

				postsWithLikesCount.Add(postik);
			}
			groupProfile.Posts = postsWithLikesCount;
			return View(groupProfile);
		}

		[Authorize]
		public async Task<IActionResult> CreateGroup()
		{
			try
			{
				ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName");
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			return View();
		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateGroup(CreateGroupModel createModel)
		{
			if (ModelState.IsValid)
			{
				CpcmGroup group = new();
				group.CpcmGroupName = createModel.CpcmGroupName;
				group.CpcmGroupAbout = createModel.CpcmGroupAbout;
				group.CpcmGroupSubject = createModel.CpcmGroupSubject;
				group.CpcmGroupCity = createModel.CpcmGroupCity;
				group.CpcmGroupNickName = createModel.CpcmGroupNickName;
				group.CpcmGroupTelNum = createModel.CpcmGroupTelNum;
				group.CpcmGroupId = Guid.NewGuid();

				string filePathGroupImage = "";
				if (createModel.CpcmGroupImage != null && createModel.CpcmGroupImage.Length > 0)
				{
					CheckIFormFile("CpcmGroupImage", createModel.CpcmGroupImage, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });
					if (ModelState.IsValid)
					{
						var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(createModel.CpcmGroupImage.FileName);
						filePathGroupImage = Path.Combine("wwwroot", "uploads", uniqueFileName);

						try
						{
							using (var fileStream = new FileStream(filePathGroupImage, FileMode.Create))
							{
								await createModel.CpcmGroupImage.CopyToAsync(fileStream);
							}
							group.CpcmGroupImage = filePathGroupImage.Replace("wwwroot", "");
						}
						catch (Exception)
						{
							group.CpcmGroupImage = null;
						}
					}
				}
				string filePathGroupCovet = "";
				if (createModel.CpcmGroupCovet != null && createModel.CpcmGroupCovet.Length > 0)
				{
					CheckIFormFile("CpcmGroupImage", createModel.CpcmGroupCovet, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });
					if (ModelState.IsValid)
					{
						var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(createModel.CpcmGroupCovet.FileName);
						filePathGroupCovet = Path.Combine("wwwroot", "uploads", uniqueFileName);

						try
						{
							using (var fileStream = new FileStream(filePathGroupCovet, FileMode.Create))
							{
								await createModel.CpcmGroupCovet.CopyToAsync(fileStream);
							}
							group.CpcmGroupCovet = filePathGroupCovet.Replace("wwwroot", "");
						}
						catch (Exception)
						{
							group.CpcmGroupCovet = null;
						}
					}
				}

				if (!ModelState.IsValid)
				{
					try
					{
						ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", createModel.CpcmGroupCity);
						ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", createModel.CpcmGroupSubject);

						return View(createModel);
					}
					catch (DbException)
					{
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
						return View("UserError");
					}
				}

				_context.CpcmGroups.Add(group);

				try
				{
					CpcmUser user = await _context.CpcmUsers.Where(u => u.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId")).FirstOrDefaultAsync();
					if (user == null)
					{
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
						return View("UserError");
					}
					CpcmGroupfollower gf = new() { CpcmUserId = user.CpcmUserId, CpcmGroupId = group.CpcmGroupId, CpcmUserRole = CpcmGroupfollower.AuthorRole };
					_context.CpcmGroupfollowers.Add(gf);

					await _context.SaveChangesAsync();
				}
				catch (DbException)
				{
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";

					if (System.IO.File.Exists(filePathGroupImage))
					{
						System.IO.File.Delete(filePathGroupImage);
					}
					if (System.IO.File.Exists(filePathGroupCovet))
					{
						System.IO.File.Delete(filePathGroupCovet);
					}
					return View("UserError");
				}
				return View(nameof(Index), new GroupFilterModel() { GroupId = group.CpcmGroupId });
			}
			else
			{
				try
				{
					ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", createModel.CpcmGroupCity);
					ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", createModel.CpcmGroupSubject);
				}
				catch (DbException)
				{
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
				}
				return View(createModel);
			}




		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> EditGroup(Guid id)
		{
			CpcmGroup? group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == id)
					.Include(g => g.CpcmGroupSubjectNavigation)
					.Include(g => g.CpcmGroupCityNavigation)
					.FirstOrDefaultAsync();
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
			if (group.CpcmGroupBanned)
			{
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Группа заблокирована";
				return View("UserError");
			}
			if (group.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}
			if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true))
			{
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Недостаточно прав";
				return View("UserError");
			}
			CreateGroupModel model = new CreateGroupModel()
			{
				GroupId = group.CpcmGroupId,
				CpcmGroupName = group.CpcmGroupName,
				CpcmGroupAbout = group.CpcmGroupAbout,
				CpcmGroupSubject = group.CpcmGroupSubject,
				CpcmGroupCity = group.CpcmGroupCity,
				CpcmGroupTelNum = group.CpcmGroupTelNum,
				CpcmGroupNickName = group.CpcmGroupTelNum
			};
			try
			{
				ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", group.CpcmGroupCity);
				ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", group.CpcmGroupSubject);
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			return View(model);

		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditGroup(EditeGroupModel model)
		{
			if (model.GroupId == null)
			{
				return StatusCode(403);
			}
			if (ModelState.IsValid)
			{
				CpcmGroup? group;
				try
				{
					group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == model.GroupId)
						.Include(g => g.CpcmGroupSubjectNavigation)
						.Include(g => g.CpcmGroupCityNavigation)
						.FirstOrDefaultAsync();
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
				if (group.CpcmGroupBanned)
				{
					Response.StatusCode = 403;
					ViewData["ErrorCode"] = 403;
					ViewData["Message"] = "Группа заблокирована";
					return View("UserError");
				}
				if (group.CpcmIsDeleted)
				{
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Группа не найдена";
					return View("UserError");
				}
				if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true))
				{
					Response.StatusCode = 403;
					ViewData["ErrorCode"] = 403;
					ViewData["Message"] = "Недостаточно прав";
					return View("UserError");
				}
				group.CpcmGroupName = model.CpcmGroupName;
				group.CpcmGroupAbout = model.CpcmGroupAbout;
				group.CpcmGroupSubject = model.CpcmGroupSubject;
				group.CpcmGroupCity = model.CpcmGroupCity;
				group.CpcmGroupNickName = model.CpcmGroupNickName;
				group.CpcmGroupTelNum = model.CpcmGroupTelNum;

				string oldImage = group.CpcmGroupImage;
				string oldCovet = group.CpcmGroupCovet;

				string filePathGroupImage = "";
				if (model.CpcmGroupImage != null && model.CpcmGroupImage.Length > 0)
				{
					CheckIFormFile("CpcmGroupImage", model.CpcmGroupImage, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });
					if (ModelState.IsValid)
					{
						var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CpcmGroupImage.FileName);
						filePathGroupImage = Path.Combine("wwwroot", "uploads", uniqueFileName);

						try
						{
							using (var fileStream = new FileStream(filePathGroupImage, FileMode.Create))
							{
								await model.CpcmGroupImage.CopyToAsync(fileStream);
							}
							group.CpcmGroupImage = filePathGroupImage.Replace("wwwroot", "");
						}
						catch (Exception)
						{
							//group.CpcmGroupImage = null;
						}
					}
				}
				string filePathGroupCovet = "";
				if (model.CpcmGroupCovet != null && model.CpcmGroupCovet.Length > 0)
				{
					CheckIFormFile("CpcmGroupImage", model.CpcmGroupCovet, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });
					if (ModelState.IsValid)
					{
						var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.CpcmGroupCovet.FileName);
						filePathGroupCovet = Path.Combine("wwwroot", "uploads", uniqueFileName);

						try
						{
							using (var fileStream = new FileStream(filePathGroupCovet, FileMode.Create))
							{
								await model.CpcmGroupCovet.CopyToAsync(fileStream);
							}
							group.CpcmGroupCovet = filePathGroupCovet.Replace("wwwroot", "");
						}
						catch (Exception)
						{
							//group.CpcmGroupCovet = null;
						}
					}
				}

				if (!ModelState.IsValid)
				{
					try
					{
						ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", model.CpcmGroupCity);
						ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", model.CpcmGroupSubject);

						return View(model);
					}
					catch (DbException)
					{
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
						return View("UserError");
					}
				}



				try
				{
					//CpcmUser user = await _context.CpcmUsers.Where(u => u.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId")).FirstOrDefaultAsync();
					//if (user == null)
					//{
					//	Response.StatusCode = 500;
					//	ViewData["ErrorCode"] = 500;
					//	ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					//	return View("UserError");
					//}
					//CpcmGroupfollower gf = new() { CpcmUserId = user.CpcmUserId, CpcmGroupId = group.CpcmGroupId, CpcmUserRole = 0 };
					//_context.CpcmGroupfollowers.Add(gf);

					await _context.SaveChangesAsync();
				}
				catch (DbException)
				{
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";

					if (System.IO.File.Exists(filePathGroupImage))
					{
						System.IO.File.Delete(filePathGroupImage);
					}
					if (System.IO.File.Exists(filePathGroupCovet))
					{
						System.IO.File.Delete(filePathGroupCovet);
					}
					return View("UserError");
				}
				return View(nameof(Index), new GroupFilterModel() { GroupId = group.CpcmGroupId });
			}
			else
			{
				try
				{
					ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", model.CpcmGroupCity);
					ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", model.CpcmGroupSubject);
				}
				catch (DbException)
				{
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
				}
				return View(model);
			}
		}

		[Authorize]

		public async Task<IActionResult> EditUserGroupRole(Guid id)
		{
			CpcmGroup? group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == id)
					.FirstOrDefaultAsync();
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
			if (group.CpcmGroupBanned)
			{
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Группа заблокирована";
				return View("UserError");
			}
			if (group.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}

			if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true))
			{
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Недостаточно прав";
				return View("UserError");
			}

			ViewData["GroupId"] = id;
			ViewData["CpcmUserRoles"] = new SelectList(await _context.CpcmGroupRoles.Where(r => r.CpcmRoleId != CpcmGroupfollower.AuthorRole).ToListAsync(), "CpcmRoleId", "CpcmRoleName");
			return View();

		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EdtiUserGroupRole(Guid userId, Guid groupId, int roleID)
		{
			CpcmGroup? group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId)
					.FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				return StatusCode(500, new { status = false, message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
			}
			if (group == null)
			{
				return StatusCode(404, new { status = false, message = "Группа не найдена" });
			}
			if (group.CpcmGroupBanned)
			{
				return StatusCode(403, new { status = false, message = "Группа заблокирована" });
			}
			if (group.CpcmIsDeleted)
			{
				return StatusCode(404, new { status = false, message = "Группа не найдена" });
			}
			if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true))
			{
				return StatusCode(403, new { status = false, message = "Недостаточно прав" });
			}


			try
			{
				var follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == userId && f.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (follower == null)
				{
					return StatusCode(404, new { status = false, message = "Пользователь не найден" });
				}
				if (roleID == 0) return StatusCode(403, new { status = false, message = "Нельзя выдать статус \"Автор\" " });
				follower.CpcmUserRole = roleID;
				await _context.SaveChangesAsync();
				return StatusCode(200);
			}
			catch (DbException)
			{
				return StatusCode(500, new { status = false, message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
			}

		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> BanUnbanGroup(Guid userId, Guid groupId)
		{

			try
			{
				var group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (group == null)
				{
					return StatusCode(404);
				}
				if (await CheckUserPrivilegeClaim("CpcmCanEditGroups", "True"))
				{
					group.CpcmGroupBanned = !group.CpcmGroupBanned;
					await _context.SaveChangesAsync();
					return StatusCode(200, new { status = true });
				}
				else
				{
					return StatusCode(403);
				}
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
		}

		public async Task<IActionResult> DelGroup(Guid groupId)
		{

			try
			{
				var group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (group == null || group.CpcmIsDeleted)
				{
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Группа не найдена";
					return View("UserError");
				}
				if (await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true))
				{
					return View();
				}
				else
				{
					Response.StatusCode = 403;
					ViewData["ErrorCode"] = 403;
					ViewData["Message"] = "Недостаточно прав";
					return View("UserError");
				}
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
		}

		public async Task<IActionResult> DelGroup(Guid groupId, bool del)
		{

			try
			{
				var group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (group == null || group.CpcmIsDeleted)
				{
					return StatusCode(404);
				}
				if (await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true))
				{
					group.CpcmIsDeleted = !group.CpcmIsDeleted;
					await _context.SaveChangesAsync();
					return StatusCode(200, new { status = true });
				}
				else
				{
					return StatusCode(403);
				}
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
		}

		public async Task<IActionResult> Followers(UserFilterModel filters)
		{
			CpcmGroup group;
			try
			{
				if (filters.NickName == null)
				{
					group = await _context.CpcmGroups.Where(c => c.CpcmGroupId == filters.GroupId).FirstOrDefaultAsync();
				}
				else
				{
					group = await _context.CpcmGroups.Where(c => c.CpcmGroupNickName == filters.NickName).FirstOrDefaultAsync();
				}
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			if (group == null || group.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найден";
				return View("UserError");
			}

			//_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
			IQueryable<CpcmUser> followerList1;
			//List<CpcmUser> followerList2;
			try
			{
				followerList1 = _context.CpcmGroupfollowers.Where(c => c.CpcmGroupId == group.CpcmGroupId).Select(c => c.CpcmUser);
				//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			//followerList1.AddRange(followerList2);
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				followerList1 = followerList1.Where(u => u.CpcmUserCity == filters.CityId);

			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				followerList1 = followerList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				followerList1 = followerList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();

			return View(followerList1);
		}
		[HttpPost]
		public async Task<IActionResult> GetNextFollowers(UserFilterModel filters)
		{
			CpcmGroup group;
			try
			{
				if (filters.NickName == null)
				{
					group = await _context.CpcmGroups.Where(c => c.CpcmGroupId == filters.GroupId).FirstOrDefaultAsync();
				}
				else
				{
					group = await _context.CpcmGroups.Where(c => c.CpcmGroupNickName == filters.NickName).FirstOrDefaultAsync();
				}
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}

			if (group == null || group.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				return StatusCode(404);
			}

			//_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
			IQueryable<CpcmUser> followerList1;
			//List<CpcmUser> followerList2;
			try
			{
				followerList1 = _context.CpcmGroupfollowers.Where(c => c.CpcmGroupId == group.CpcmGroupId && c.CpcmUserId.CompareTo(filters.lastId) > 0).Select(c => c.CpcmUser);
				//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				followerList1 = followerList1.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				followerList1 = followerList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				followerList1 = followerList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
			//followerList1.AddRange(followerList2);

			return Json(followerList1);
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> FollowUnfollow(Guid id)
		{
			CpcmGroup group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == id).FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
			if (group == null || group.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				return StatusCode(404);
			}

			try
			{
				var follow = await _context.CpcmGroupfollowers.Where(f => f.CpcmGroupId == group.CpcmGroupId && f.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId")).Include(f=>f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
				if(follow == null)
				{
					CpcmGroupfollower ff = new() { CpcmGroupId = group.CpcmGroupId, CpcmUserId = Guid.Parse(User.FindFirstValue("CpcmUserId")), CpcmUserRole = CpcmGroupfollower.FollowerRole };
					_context.CpcmGroupfollowers.Add(ff);
				}
				else
				{
					_context.CpcmGroupfollowers.Remove(follow);
				}
				await _context.SaveChangesAsync();
				return StatusCode(200, new { status = true });
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
		}

		[HttpGet]
		public async Task<IActionResult> FindGroups(GroupFilterModel filters)
		{
			var query = _context.CpcmGroups.AsQueryable();
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				query = query.Where(u => u.CpcmGroupCity == filters.CityId);

			}
			if (!string.IsNullOrEmpty(filters.Name))
			{
				//ViewData["scgoolId"]=schoolId;
				//groupsList = groupsList.Where(u => u.CpcmGroupName == filters.Name);
				query = query.Where(u => EF.Functions.Like(u.CpcmGroupName, $"%{filters.Name}%"));
			}
			if (filters.SubjectID.HasValue)
			{
				//ViewData["universityId"] = universityId;
				query = query.Where(u => u.CpcmGroupSubject == filters.SubjectID);
			}

			try
			{
				var rez = await query.OrderBy(u => u.CpcmGroupId).Take(10).ToListAsync();
				return View(rez);
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
				//return StatusCode(500);
			}
		}
		[HttpPost]
		public async Task<IActionResult> FindNextGroups(GroupFilterModel filters)
		{
			var query = _context.CpcmGroups.AsQueryable();
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				query = query.Where(u => u.CpcmGroupCity == filters.CityId);

			}
			if (!string.IsNullOrEmpty(filters.Name))
			{
				//ViewData["scgoolId"]=schoolId;
				//groupsList = groupsList.Where(u => u.CpcmGroupName == filters.Name);
				query = query.Where(u => EF.Functions.Like(u.CpcmGroupName, $"%{filters.Name}%"));
			}
			if (filters.SubjectID.HasValue)
			{
				//ViewData["universityId"] = universityId;
				query = query.Where(u => u.CpcmGroupSubject == filters.SubjectID);
			}

			try
			{
				var rez = await query.OrderBy(u => u.CpcmGroupId).Where(g=>g.CpcmGroupId.CompareTo(filters.lastId)>0).Take(10).ToListAsync();
				return View(rez);
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
				//return StatusCode(500);
			}
		}

		public async Task<IActionResult> CreatePost(Guid groupId)
		{
			if (!await CheckOnlyGroupPrivelege("CpcmCanMakePost", true))
			{
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Недостаточно прав";
				return View("UserError");
			}
			GroupPostModel groupPost = new() { GroupId = groupId };
			return View(groupPost);
		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreatePost(GroupPostModel groupPost)
		{
			if (!await CheckOnlyGroupPrivelege("CpcmCanMakePost", true))
			{
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Недостаточно прав";
				return View("UserError");
			}

			if (ModelState.IsValid)
			{
				CpcmPost post = new CpcmPost();

				post.CpcmPostText = groupPost.Text.Trim();
				post.CpcmPostId = Guid.NewGuid();
				post.CpcmPostFather = groupPost.PostFatherId;
				post.CpcmPostCreationDate = DateTime.UtcNow;
				if (groupPost.Published == null)
				{
					post.CpcmPostPublishedDate = post.CpcmPostCreationDate;
				}
				else
				{
					post.CpcmPostPublishedDate = groupPost.Published;
				}

				//post.CpcmUserId = Guid.Parse(User.FindFirst(c => c.Type == "CpcmUserId").Value);
				post.CpcmGroupId = groupPost.GroupId;

				List<string> filePaths = new List<string>();
				List<CpcmImage> images = new List<CpcmImage>();

				if (groupPost.PostFatherId != null)
				{
					int i = 0;
					if (groupPost.Files != null)
					{
						foreach (IFormFile file in groupPost.Files)
						{
							CheckIFormFile("Files", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

							if (!ModelState.IsValid)
							{
								return View(groupPost);

							}

							string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
							filePaths.Add(Path.Combine("wwwroot", "uploads", uniqueFileName));

							try
							{
								using (var fileStream = new FileStream(filePaths.Last(), FileMode.Create))
								{
									await file.CopyToAsync(fileStream);
								}
								CpcmImage image = new CpcmImage();
								image.CpcmImageId = Guid.NewGuid();
								image.CpcmPostId = post.CpcmPostId;
								image.CpcmImagePath = filePaths.Last().Replace("wwwroot", "");
								image.CpcmImageOrder = 0;
								i++;

								images.Add(image);
							}
							catch (Exception ex)
							{
								Response.StatusCode = 500;
								ViewData["ErrorCode"] = 500;
								ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
								return View(groupPost);
							}

						}
						post.CpcmImages = images;
					}
					_context.CpcmImages.AddRange(images);
				}
				_context.CpcmPosts.Add(post);
				try
				{
					if (groupPost.PostFatherId != null)
					{
						var fatherPost = await _context.CpcmPosts.Where(p => p.CpcmPostId == groupPost.PostFatherId).FirstOrDefaultAsync();
						if (fatherPost == null || fatherPost.CpcmPostPublishedDate < DateTime.UtcNow)
						{
							return StatusCode(200, new { status = false, message = "Нельзя репостить неопубликованный пост" });
						}
						if (fatherPost.CpcmPostBanned)
						{
							return StatusCode(200, new { status = false, message = "Нельзя репостить этот пост" });
						}
						if (fatherPost.CpcmIsDeleted)
						{
							return StatusCode(404, new { message = "Не найден родительский пост" });
						}
					}
					await _context.SaveChangesAsync();
				}
				catch (DbException)
				{
					foreach (var item in filePaths)
					{
						if (System.IO.File.Exists(item))
						{
							System.IO.File.Delete(item);
						}
					}
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View(groupPost); // TODO Продумать место для сохранения еррора
				}

				if (groupPost.PostFatherId != null)
				{
					return View("Index");
				}
				else
				{
					return StatusCode(200, new { status = true });
				}

			}
			if (groupPost.PostFatherId == null)
			{
				return View(groupPost);
			}
			else
			{
				return StatusCode(200, new { status = false, message = "Репост имел неккоректный вид. Возможно вы попытались прикрепить файлы. Однако этого нельзя делать для репостов." });
			}
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> DeletePost(Guid postGuid)
		{

			CpcmPost? post = null;
			try
			{
				post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
			if (post == null||!post.CpcmIsDeleted)
			{
				return StatusCode(404);
			}
			if (post.CpcmPostBanned)
			{
				return StatusCode(403);
			}

			if (!await CheckOnlyGroupPrivelege("CpcmCanDelPost", true))
			{
				return StatusCode(403);
			}


			post.CpcmIsDeleted = true;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
			return StatusCode(200, new { status = true });
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> BanUnbanPost(Guid id)
		{
			if (!await CheckUserPrivilegeClaim("CpcmCanDelUsersPosts", "True"))
			{
				return StatusCode(403);
			}
			try
			{
				var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == id && c.CpcmPostPublishedDate < DateTime.UtcNow).FirstOrDefaultAsync();
				if (post == null || post.CpcmIsDeleted == true)
				{
					return StatusCode(404);
				}
				post.CpcmPostBanned = !post.CpcmPostBanned;
				await _context.SaveChangesAsync();
				return StatusCode(200, new { status = true });
			}
			catch (DbException)
			{
				return StatusCode(500);
			}
		}




		public async Task<IActionResult> CheckCreateNickName(string CpcmGroupNickName)
		{
			if (CpcmGroupNickName == null || CpcmGroupNickName.All(char.IsWhiteSpace) || CpcmGroupNickName == string.Empty)
			{
				return Json(true);
			}
			CpcmGroupNickName = CpcmGroupNickName.Trim();
			if (CpcmGroupNickName.Contains("admin") || CpcmGroupNickName.Contains("webmaster") || CpcmGroupNickName.Contains("abuse"))
			{
				return Json(false);
			}

			bool rez = false;
			try
			{
				rez = !await _context.CpcmGroups.AnyAsync(e => e.CpcmGroupNickName == CpcmGroupNickName);
			}
			catch (DbException)
			{
				return Json(false);
			}
			return Json(rez);
		}
		public async Task<IActionResult> CheckCreateNickName(string CpcmGroupNickName, Guid GroupId)
		{
			if (CpcmGroupNickName == null || CpcmGroupNickName.All(char.IsWhiteSpace) || CpcmGroupNickName == string.Empty)
			{
				return Json(true);
			}
			CpcmGroupNickName = CpcmGroupNickName.Trim();
			var authFactor = bool.Parse(User.FindFirstValue("CpcmCanEditGroups"));
			if (CpcmGroupNickName.Contains("admin") || CpcmGroupNickName.Contains("webmaster") || CpcmGroupNickName.Contains("abuse") && !authFactor)
			{
				return Json(false);
			}

			bool rez = false;
			try
			{
				rez = !await _context.CpcmGroups.AnyAsync(e => e.CpcmGroupNickName == CpcmGroupNickName && e.CpcmGroupId!=GroupId);
			}
			catch (DbException)
			{
				return Json(false);
			}
			return Json(rez);
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
		private bool CheckIFormFileContent(IFormFile cpcmUserImage, string[] permittedTypes)
		{
			if (cpcmUserImage != null && permittedTypes.Contains(cpcmUserImage.ContentType))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		private bool CheckIFormFileSize(IFormFile cpcmUserImage, int size)
		{

			if (cpcmUserImage.Length > 0 && cpcmUserImage.Length < size)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		private bool CheckIFormFile(string FormFieldName, IFormFile file, int size, string[] permittedTypes)
		{
			bool status = true;
			if (!CheckIFormFileContent(file, permittedTypes))
			{
				ModelState.AddModelError(FormFieldName, "Допустимые типы файлов: png, jpeg, jpg, gif");
				status = false;
			}
			if (!CheckIFormFileSize(file, size))
			{
				ModelState.AddModelError(FormFieldName, $"Максимальный размер файла: {size / 1024} Кбайт");
				status = false;
			}
			return status;
		}

		
		private async Task<bool> CheckOnlyGroupPrivelege(string propertyName, object propertyValue)
		{
			CpcmGroupfollower? follower;
			try
			{
				follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId")).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
				
			}
			catch (DbException)
			{
				return (false);
			}
			if (follower == null)
			{
				return false;
			}
			Type type = follower.CpcmUserRoleNavigation.GetType();
			PropertyInfo propertyInfo = type.GetProperty(propertyName);
			var value = propertyInfo.GetValue(follower.CpcmUserRoleNavigation);
			bool rezFollower = propertyValue != null && propertyValue.Equals(value);
			return rezFollower;
		}
		private async Task<bool> CheckUserPrivilege(string propertyName, object propertyValue, string userPropName, object userPropVal, Guid id)
		{
			CpcmGroupfollower? follower;
			CpcmUser? user;
			try
			{
				follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == id).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
				user = await _context.CpcmUsers.Where(f => f.CpcmUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				return(false);
			}
			if (follower == null)
			{
				return false;
			}

			Type type = follower.CpcmUserRoleNavigation.GetType();
			PropertyInfo propertyInfo = type.GetProperty(propertyName);
			var value = propertyInfo.GetValue(follower.CpcmUserRoleNavigation);
			bool rezFollower = propertyValue != null && propertyValue.Equals(value);

			type = user.CpcmUserRoleNavigation.GetType();
			propertyInfo = type.GetProperty(userPropName);
			value = propertyInfo.GetValue(user.CpcmUserRoleNavigation);
			bool rezUser = userPropVal != null && userPropVal.Equals(value);

			return rezFollower || rezUser;
			
		}

		private async Task<bool> CheckUserPrivilege(string propertyName, object propertyValue, string userPropName, object userPropVal)
		{
			CpcmGroupfollower? follower;
			CpcmUser? user;
			Guid id = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
			try
			{
				follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == id).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
				user = await _context.CpcmUsers.Where(f => f.CpcmUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				return (false);
			}
			if (follower == null)
			{
				return false;
			}

			Type type = follower.CpcmUserRoleNavigation.GetType();
			PropertyInfo propertyInfo = type.GetProperty(propertyName);
			var value = propertyInfo.GetValue(follower.CpcmUserRoleNavigation);
			bool rezFollower = propertyValue != null && propertyValue.Equals(value);

			type = user.CpcmUserRoleNavigation.GetType();
			propertyInfo = type.GetProperty(userPropName);
			value = propertyInfo.GetValue(user.CpcmUserRoleNavigation);
			bool rezUser = userPropVal != null && userPropVal.Equals(value);

			return rezFollower || rezUser;

		}
		//[Obsolete("Не использовать. Поведение некорректное",true)]
		//private async Task<bool> CheckUserPrivilege(string propertyName, object propertyValue)
		//{
		//	CpcmUser? user;
		//	try
		//	{
		//		user = await _context.CpcmUsers.Where(f => f.CpcmUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();

		//	}
		//	catch (DbException)
		//	{
		//		return (false);
		//	}
		//	if (user == null)
		//	{
		//		return false;
		//	}

		//	Type type = user.CpcmUserRoleNavigation.GetType();
		//	PropertyInfo propertyInfo = type.GetProperty(propertyName);
		//	var value = propertyInfo.GetValue(user.CpcmUserRoleNavigation);

		//	return propertyValue != null && propertyValue.Equals(value);
		//}
		private async Task<bool> CheckUserPrivilegeClaim(string claimType, string claimValue)
		{
			var authFactor = HttpContext.User.FindFirst(c => c.Type == claimType && c.Value == claimValue);
			if (authFactor == null)
			{
				return false;
			}
			return true;
		}

	}
}
