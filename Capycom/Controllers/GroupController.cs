using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Data.Common;
using System.Reflection;
using System.Security.Claims;

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
			if (User.Identity.IsAuthenticated)
				Log.Information("Пользовать {user} просматривает группу {filter}", User.FindFirstValue("CpcmUserId"), filter);
			else
				Log.Information("Неавторизирвоанный клиент {client} просматривает группу {filter}", HttpContext.Connection, filter);
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
			catch (DbException ex)
			{
				Log.Error(ex,"Ошибка при получении группы из базы данных", filter);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			if (group == null)
			{
				Log.Warning("Группа не найдена", filter);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}
			if (group.CpcmIsDeleted)
			{
				Log.Information("Попытка обратиться к удалённой группе ", filter);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}
			if (group.CpcmGroupBanned)
			{
				Log.Information("Попытка обратиться к заблокированной группе", filter);
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Группа заблокирована";
				return View("UserError");
			}



			List<CpcmPost> posts;
			long liked;
			CpcmGroupfollower follower = null;
			try
			{
				posts = await _context.CpcmPosts.Where(c => c.CpcmGroupId == group.CpcmGroupId && c.CpcmPostPublishedDate < DateTime.UtcNow).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).AsNoTracking().Take(10).ToListAsync();
				if (User.Identity.IsAuthenticated)
				{
					follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmGroupId == group.CpcmGroupId && f.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId")).FirstOrDefaultAsync(); 
				}
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении постов или подписчиков группы из базы данных", filter);
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
				
				try
				{
					postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
					long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
					long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
					postik.LikesCount = likes;
					postik.RepostsCount = reposts;
				}
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при получении лайков и репостов поста из базы данных", filter);
				}
				

				if (User.Identity.IsAuthenticated)
				{
					var guid = Guid.Parse(User.FindFirstValue("CpcmUserId"));
					try
					{
						
						liked = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId} && CPCM_UserID = {guid}").CountAsync();
					}
					catch (DbException ex)
					{
						Log.Error(ex, "Ошибка при получении информации о лайке поста пользователем {user} из базы данных", guid, filter);
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
				if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
				{
					string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
					if (timezoneOffsetCookie != null)
					{
						if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
						{
							TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

							postik.CpcmPostPublishedDate -= offset;

						}
					}
				}


				postsWithLikesCount.Add(postik);
			}
			groupProfile.Posts = postsWithLikesCount;
			return View(groupProfile);
		}

		public async Task<IActionResult> GetNextPosts(Guid groupId, Guid lastPostId)
		{
			List<CpcmPost> posts;
			ICollection<CpcmPost> postsWithLikesCount = new List<CpcmPost>();
			try
			{
				var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == lastPostId).FirstOrDefaultAsync();
				if (post == null)
				{
					return StatusCode(404);
				}


				long liked;
				posts = await _context.CpcmPosts.Where(c => c.CpcmGroupId == groupId).Where(c => c.CpcmPostPublishedDate < post.CpcmPostPublishedDate && c.CpcmPostPublishedDate < DateTime.UtcNow).AsNoTracking().Take(10).ToListAsync();

				foreach (var postik in posts)
				{
					try
					{
						postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
						long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
						long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
						postik.LikesCount = likes;
						postik.RepostsCount = reposts;
					}
					catch (DbException ex)
					{
						Log.Error(ex, "Не удалось выгрузить родительские посты, лайки и репосты для следующий постов за {lastPostId}", lastPostId);
						throw;
					}



					if (User.Identity.IsAuthenticated)
					{
						try
						{
							liked = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId} && CPCM_UserID = {postik.CpcmUserId}").CountAsync();
						}
						catch (DbException)
						{
							return StatusCode(500);
						}
						if (liked > 0)
							postik.IsLiked = true;
						else
							postik.IsLiked = false;

					}


					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								postik.CpcmPostPublishedDate -= offset;

							}
						}
					}
				}

				
			}
			catch (DbException ex)
			{
				Log.Fatal(ex, "Ошибка при получении постов из базы данных", groupId, lastPostId);
				return StatusCode(500);
			}
			return PartialView(postsWithLikesCount);
		}

		[Authorize]
		public async Task<IActionResult> CreateGroup()
		{
			try
			{
				ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				ViewData["CpcmGroupSubject"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName");
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка городов и тем из базы данных");
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
			Log.Information("Попытка создания группы {createModel} пользователем {user}", createModel, HttpContext.User.FindFirstValue("CpcmUserId"));
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
						catch (Exception ex)
						{
							Log.Error(ex, "Ошибка при загрузке изображения группы", filePathGroupImage);
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
						catch (Exception ex)
						{
							Log.Error(ex, "Ошибка при загрузке фона группы", filePathGroupCovet);
							group.CpcmGroupCovet = null;
						}
					}
				}

				if (!ModelState.IsValid)
				{
					try
					{
						ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", createModel.CpcmGroupCity);
						ViewData["CpcmGroupSubject"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", createModel.CpcmGroupSubject);

						return View(createModel);
					}
					catch (DbException ex)
					{	
						Log.Error(ex, "Ошибка при получении списка городов и тем из базы данных");
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
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при добавлении группы в базу данных", group, HttpContext.User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";

					try
					{
						if (System.IO.File.Exists(filePathGroupImage))
						{
							System.IO.File.Delete(filePathGroupImage);
						}
						if (System.IO.File.Exists(filePathGroupCovet))
						{
							System.IO.File.Delete(filePathGroupCovet);
						}
					}
					catch (IOException exx)
					{
						Log.Error(exx, "Ошибка при удалении изображения группы", filePathGroupImage, filePathGroupCovet);
					}
					return View("UserError");
				}
				return View(nameof(Index), new GroupFilterModel() { GroupId = group.CpcmGroupId });
			}
			else
			{
				Log.Information("Невалидная модель при создании группы", createModel, HttpContext.User.FindFirstValue("CpcmUserId"));
				try
				{
					ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", createModel.CpcmGroupCity);
					ViewData["CpcmGroupSubject"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", createModel.CpcmGroupSubject);
				}
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при получении списка городов и тем из базы данных");
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
			Log.Information("Попытка редактирования группы {id} пользователем {user}", id, HttpContext.User.FindFirstValue("CpcmUserId"));
			CpcmGroup? group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == id)
					.Include(g => g.CpcmGroupSubjectNavigation)
					.Include(g => g.CpcmGroupCityNavigation)
					.FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении группы из базы данных", id);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			if (group == null)
			{
				Log.Error("Группа не найдена", id);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}
			if (group.CpcmGroupBanned)
			{
				Log.Information("Попытка обратиться к заблокированной группе", id);
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Группа заблокирована";
				return View("UserError");
			}
			if (group.CpcmIsDeleted)
			{
				Log.Information("Попытка обратиться к удалённой группе", id);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}
			if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true,id))
			{
				Log.Warning("Недостаточно прав для редактирования группы", id);
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
				ViewData["CpcmGroupSubject"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", group.CpcmGroupSubject);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка городов и тем из базы данных");
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
				Log.Warning("Попытка редактирования группы без указания идентификатора", model);
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
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при получении группы из базы данных", model);
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
				}
				if (group == null)
				{
					Log.Warning("Группа не найдена", model);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Группа не найдена";
					return View("UserError");
				}
				if (group.CpcmGroupBanned)
				{
					Log.Information("Попытка обратиться к заблокированной группе", model);
					Response.StatusCode = 403;
					ViewData["ErrorCode"] = 403;
					ViewData["Message"] = "Группа заблокирована";
					return View("UserError");
				}
				if (group.CpcmIsDeleted)
				{
					Log.Information("Попытка обратиться к удалённой группе", model);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Группа не найдена";
					return View("UserError");
				}
				if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true,model.GroupId))
				{
					Log.Warning("Недостаточно прав для редактирования группы", model);
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
						catch (Exception ex)
						{
							Log.Error(ex,"Ошибка при загрузке изображения группы", filePathGroupImage, model.CpcmGroupImage);
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
						catch (Exception ex)
						{
							//group.CpcmGroupCovet = null;
							Log.Error(ex, "Ошибка при загрузке фона группы", filePathGroupCovet, model.CpcmGroupCovet);
						}
					}
				}

				if (!ModelState.IsValid)
				{
					try
					{
						ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", model.CpcmGroupCity);
						ViewData["CpcmGroupSubject"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", model.CpcmGroupSubject);

						return View(model);
					}
					catch (DbException ex)
					{
						Log.Error(ex, "Ошибка при получении списка городов и тем из базы данных");
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
				catch (DbUpdateConcurrencyException ex)
				{

					Log.Error(ex, "Ошибка при обновлении группы в базе данных - конкуренция", group, HttpContext.User.FindFirstValue("CpcmUserId"));
					try
					{
						if (System.IO.File.Exists(filePathGroupImage))
						{
							System.IO.File.Delete(filePathGroupImage);
						}
						if (System.IO.File.Exists(filePathGroupCovet))
						{
							System.IO.File.Delete(filePathGroupCovet);
						}
					}
					catch (IOException exx)
					{
						Log.Error(exx, "Ошибка при удалении изображения группы", filePathGroupImage, filePathGroupCovet);
					}
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Не удалось сохранить изменения. Возможно кто-то попытался внести изменение до вас.";
					return View("UserError");
				}
				catch (DbUpdateException ex)
				{
					Log.Fatal(ex, "Ошибка при обновлении группы в базе данных", group, HttpContext.User.FindFirstValue("CpcmUserId"));
					try
					{
						if (System.IO.File.Exists(filePathGroupImage))
						{
							System.IO.File.Delete(filePathGroupImage);
						}
						if (System.IO.File.Exists(filePathGroupCovet))
						{
							System.IO.File.Delete(filePathGroupCovet);
						}
					}
					catch (IOException exx)
					{
						Log.Error(exx, "Ошибка при удалении изображения группы", filePathGroupImage, filePathGroupCovet);
					}
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Не удалось сохранить изменения. Пожалуйста, обратитесь в слежбу поддержки если это повторится вновь";
					return View("UserError");
				}
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при добавлении группы в базу данных", group, HttpContext.User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";

					try
					{
						if (System.IO.File.Exists(filePathGroupImage))
						{
							System.IO.File.Delete(filePathGroupImage);
						}
						if (System.IO.File.Exists(filePathGroupCovet))
						{
							System.IO.File.Delete(filePathGroupCovet);
						}
					}
					catch (IOException exx)
					{
						Log.Error(exx, "Ошибка при удалении изображения группы", filePathGroupImage, filePathGroupCovet);
					}
					return View("UserError");
				}
				return View(nameof(Index), new GroupFilterModel() { GroupId = group.CpcmGroupId });
			}
			else
			{
				Log.Information("Невалидная модель при редактировании группы", model);
				try
				{
					ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", model.CpcmGroupCity);
					ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmGroupsubjects.ToListAsync(), "CpcmSubjectId", "CpcmSubjectName", model.CpcmGroupSubject);
				}
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при получении списка городов и тем из базы данных");
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
			Log.Information("Попытка редактирования роли пользователя в группе {id} пользователем {user}", id, HttpContext.User.FindFirstValue("CpcmUserId"));
			CpcmGroup? group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == id)
					.FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении группы из базы данных", id);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			if (group == null)
			{
				Log.Warning("Группа не найдена", id);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}
			if (group.CpcmGroupBanned)
			{
				Log.Information("Попытка обратиться к заблокированной группе", id);
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Группа заблокирована";
				return View("UserError");
			}
			if (group.CpcmIsDeleted)
			{
				Log.Information("Попытка обратиться к удалённой группе", id);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Группа не найдена";
				return View("UserError");
			}

			if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true, id))
			{
				Log.Warning("Недостаточно прав для редактирования группы", id);
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Недостаточно прав";
				return View("UserError");
			}

			ViewData["GroupId"] = id;
			try
			{
				ViewData["CpcmUserRoles"] = new SelectList(await _context.CpcmGroupRoles.Where(r => r.CpcmRoleId != CpcmGroupfollower.AuthorRole).ToListAsync(), "CpcmRoleId", "CpcmRoleName");

			}
			catch (DbException ex)
			{		
				Log.Error(ex, "Ошибка при получении списка ролей из базы данных");
			}
			return View();

		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EdtiUserGroupRole(Guid userId, Guid groupId, int roleID)
		{
			Log.Information("Попытка редактирования роли пользователя {userId} в группе {groupId} пользователем {user}", userId, groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			CpcmGroup? group;
			try
			{
				group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId)
					.FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении группы из базы данных", groupId);
				return StatusCode(500, new { status = false, message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
			}
			if (group == null)
			{
				Log.Warning("Группа не найдена", groupId);
				return StatusCode(404, new { status = false, message = "Группа не найдена" });
			}
			if (group.CpcmGroupBanned)
			{
				Log.Warning("Попытка обратиться к заблокированной группе", groupId);
				return StatusCode(403, new { status = false, message = "Группа заблокирована" });
			}
			if (group.CpcmIsDeleted)
			{
				Log.Warning("Попытка обратиться к удалённой группе", groupId);
				return StatusCode(404, new { status = false, message = "Группа не найдена" });
			}
			if (!await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true, groupId))
			{
				Log.Warning("Недостаточно прав для редактирования группы", groupId);
				return StatusCode(403, new { status = false, message = "Недостаточно прав" });
			}


			try
			{
				var follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == userId && f.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (follower == null)
				{
					Log.Warning("Пользователь не найден", userId);
					return StatusCode(404, new { status = false, message = "Пользователь не найден" });
				}
				if (roleID == 0)
				{
					Log.Warning("Попытка выдать статус \"Автор\"", userId);
					return StatusCode(403, new { status = false, message = "Нельзя выдать статус \"Автор\" " });
				}
					
				follower.CpcmUserRole = roleID;
				await _context.SaveChangesAsync();
				return StatusCode(200);
			}
			catch(DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при обновлении роли пользователя в группе", userId, groupId);
				return StatusCode(500, new { status = false, message = "Не удалось сохранить изменения. Возможно кто-то попытался внести изменение до вас." });
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при обновлении роли пользователя в группе", userId, groupId);
				return StatusCode(500, new { status = false, message = "Не удалось сохранить изменения. Пожалуйста, обратитесь в службу поддержки если это повторится вновь" });
			}	
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при обновлении роли пользователя в группе", userId, groupId);
				return StatusCode(500, new { status = false, message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
			}

		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> BanUnbanGroup(Guid userId, Guid groupId)
		{
			Log.Information("Попытка заблокировать/разблокировать группу {groupId} пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			try
			{
				var group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (group == null)
				{
					Log.Warning("Группа не найдена", groupId);
					return StatusCode(404);
				}
				if (await CheckUserPrivilegeClaim("CpcmCanEditGroups", "True"))
				{
					group.CpcmGroupBanned = !group.CpcmGroupBanned;
					await _context.SaveChangesAsync();
					Log.Information("Группа {groupId} заблокирована/разблокирована пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
					return StatusCode(200, new { status = true });
				}
				else
				{
					Log.Warning("Недостаточно прав для редактирования группы", groupId);
					return StatusCode(403);
				}
			}
			catch(DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при обновлении статуса группы", groupId);
				return StatusCode(500);
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при обновлении статуса группы", groupId);
				return StatusCode(500);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при обновлении статуса группы", groupId);
				return StatusCode(500);
			}
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> DelGroup(Guid groupId)
		{
			Log.Information("Попытка удаления группы {groupId} пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			try
			{
				var group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (group == null || group.CpcmIsDeleted)
				{
					Log.Warning("Группа для удаления не найдена", groupId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Группа не найдена";
					return View("UserError");
				}
				if (await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true, groupId))
				{
					return View();
				}
				else
				{
					Log.Warning("Недостаточно прав для удаления группы", groupId);
					Response.StatusCode = 403;
					ViewData["ErrorCode"] = 403;
					ViewData["Message"] = "Недостаточно прав";
					return View("UserError");
				}
			}
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при удалении группы - конкурентность", groupId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка при удалении группы. Возможно кто-то уже попытался удалить группу.";
				return View("UserError");
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при удалении группы", groupId);
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка соединения с сервером, повторите попытку позже";
				return View("UserError");
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при удалении группы", groupId);
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> DelGroup(Guid groupId, bool del)
		{
			Log.Information("Попытка удаления группы {groupId} пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			try
			{
				var group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == groupId).FirstOrDefaultAsync();
				if (group == null || group.CpcmIsDeleted)
				{
					Log.Warning("Группа для удаления не найдена", groupId);
					return StatusCode(404);
				}
				if (await CheckUserPrivilege("CpcmCanEditGroup", true, "CpcmCanEditGroups", true, groupId))
				{

					group.CpcmIsDeleted = !group.CpcmIsDeleted;
					await _context.SaveChangesAsync();
					Log.Information("Группа {groupId} удалена пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
					return StatusCode(200, new { status = true });
				}
				else
				{
					Log.Warning("Недостаточно прав для удаления группы", groupId);
					return StatusCode(403);
				}
			}
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при удалении группы - конкурентность", groupId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка при удалении группы. Возможно кто-то уже попытался удалить группу.";
				return View("UserError");
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при удалении группы", groupId);
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка соединения с сервером, повторите попытку позже";
				return View("UserError");
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при удалении группы", groupId);
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
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении группы из базы данных", filters);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			if (group == null || group.CpcmIsDeleted)
			{
				Log.Warning("Группа не найдена", filters);
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
				ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName");
				ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName");
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка городов, школ и университетов из базы данных", filters);
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

			try
			{
				var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();

				return View(followerList1);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка подписчиков группы из базы данных", filters);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
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
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении группы из базы данных", filters);
				Response.StatusCode = 500;
				return StatusCode(500);
			}

			if (group == null || group.CpcmIsDeleted)
			{
				Log.Warning("Группа не найдена", filters);
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
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка подписчиков группы из базы данных", filters);
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

			try
			{
				var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
				//followerList1.AddRange(followerList2);

				return PartialView(followerList1);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка подписчиков группы из базы данных", filters);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
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
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении группы из базы данных", id);
				return StatusCode(500);
			}
			if (group == null || group.CpcmIsDeleted)
			{
				Log.Warning("Группа не найдена", id);
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
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при обновлении подписки на группу", id);
				return StatusCode(500 , new {message = "не удалось отписаться/подписаться. Возможно вы уже сделали это действие с другого устройства"});
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при обновлении подписки на группу", id);
				return StatusCode(500);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при обновлении подписки на группу", id);
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
				ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				var rez = await query.OrderBy(u => u.CpcmGroupId).Take(10).ToListAsync();
				return View(rez);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка групп из базы данных", filters);
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
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении списка групп из базы данных", filters);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
				//return StatusCode(500);
			}
		}
		[Authorize]
		public async Task<IActionResult> CreatePost(Guid groupId)
		{
			if (!await CheckOnlyGroupPrivelege("CpcmCanMakePost", true, groupId))
			{
				Log.Warning("Недостаточно прав для создания поста", groupId);
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
			Log.Information("Попытка создания поста в группе {groupId} пользователем {user}", groupPost.GroupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			if (!await CheckOnlyGroupPrivelege("CpcmCanMakePost", true, groupPost.GroupId))
			{
				Log.Warning("Недостаточно прав для создания поста", groupPost.GroupId);
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
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								post.CpcmPostPublishedDate += offset;

							}
						}
					}
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
							catch (IOException ex)
							{
								Log.Error(ex, "Ошибка при сохранении файла", groupPost);
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
				catch (DbException ex)
				{
					try
					{
						foreach (var item in filePaths)
						{
							if (System.IO.File.Exists(item))
							{
								System.IO.File.Delete(item);
							}
						}
					}
					catch (IOException)
					{
						Log.Error("Ошибка при удалении файла", groupPost);
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
			Log.Warning("Ошибка валидации модели", groupPost);
			if (groupPost.PostFatherId == null)
			{
				return View(groupPost);
			}
			else
			{
				Log.Warning("Попытка репоста с неккоректной моделью", groupPost);
				return StatusCode(200, new { status = false, message = "Репост имел неккоректный вид. Возможно вы попытались прикрепить файлы. Однако этого нельзя делать для репостов." });
			}
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> DeletePost(Guid postGuid, Guid groupId)
		{
			Log.Information("Попытка удаления поста {postGuid} пользователем {user}", postGuid, HttpContext.User.FindFirstValue("CpcmUserId"));
			CpcmPost? post = null;
			try
			{
				post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении поста из базы данных", postGuid);
				return StatusCode(500);
			}
			if (post == null||!post.CpcmIsDeleted)
			{
				Log.Warning("Пост не найден или уже удалён", postGuid, post.CpcmIsDeleted);
				return StatusCode(404);
			}
			if (post.CpcmPostBanned)
			{
				Log.Warning("Попытка удаления заблокированного поста", postGuid);
				return StatusCode(403);
			}

			if (!await CheckOnlyGroupPrivelege("CpcmCanDelPost", true, groupId))
			{
				Log.Warning("Недостаточно прав для удаления поста", postGuid);
				return StatusCode(403);
			}


			post.CpcmIsDeleted = true;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при удалении поста - конкурентность", postGuid);
				return StatusCode(500, new { message ="Обнаружена опытка удалить пост, который кто-то редактировал"});
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при удалении поста", postGuid);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при удалении поста", postGuid);
				return StatusCode(500);
			}
			return StatusCode(200, new { status = true });
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> BanUnbanPost(Guid id)
		{
			Log.Information("Попытка забанить/разбанить пост {postGuid} пользователем {user}", id, HttpContext.User.FindFirstValue("CpcmUserId"));
			if (!await CheckUserPrivilegeClaim("CpcmCanDelUsersPosts", "True"))
			{
				Log.Warning("Недостаточно прав для забана/разбана поста", id);
				return StatusCode(403);
			}
			try
			{
				var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == id && c.CpcmPostPublishedDate < DateTime.UtcNow).FirstOrDefaultAsync();
				if (post == null || post.CpcmIsDeleted == true)
				{
					Log.Warning("Пост не найден или уже удалён", id, post.CpcmIsDeleted);
					return StatusCode(404);
				}
				post.CpcmPostBanned = !post.CpcmPostBanned;
				await _context.SaveChangesAsync();
				return StatusCode(200, new { status = true });
			}
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при забане/разбане поста - конкурентность", id);
				return StatusCode(500, new { message = "Обнаружена опытка забанить/разбанить пост, который кто-то редактировал" });
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Ошибка при забане/разбане поста", id);
				return StatusCode(500);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при забане/разбане поста", id);
				return StatusCode(500);
			}
		}


		[Authorize]
		public async Task<IActionResult> EditPost(Guid postGuid, Guid groupId)
		{
			Log.Information("Попытка редактирования поста {postGuid} пользователем {user}", postGuid, HttpContext.User.FindFirstValue("CpcmUserId"));
			CpcmPost? post = null;
			try
			{
				post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid && c.CpcmGroupId==groupId).FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении поста из базы данных", postGuid);
				return StatusCode(500);
			}

			if (post == null)
			{
				Log.Warning("Пост не найден", postGuid);
				return StatusCode(404);
			}
			if (post.CpcmIsDeleted)
			{
				Log.Warning("Пост удалён", postGuid);
				return StatusCode(404);
			}
			if (!await CheckOnlyGroupPrivelege("CpcmCanEditPost", true, groupId) || post.CpcmPostBanned)
			{
				Log.Warning("Недостаточно прав для редактирования поста", postGuid, HttpContext.User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 403;
				ViewData["ErrorCode"] = 403;
				ViewData["Message"] = "Недостаточно прав";
				return View("UserError");
			}


			GroupPostEditModel model = new GroupPostEditModel();
			model.Id = post.CpcmPostId;
			model.GroupId = groupId;
			model.Text = post.CpcmPostText;
			model.CpcmImages = post.CpcmImages;
			model.NewPublishDate = post.CpcmPostPublishedDate;
			return View(model);
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPost(GroupPostEditModel editPost) 
		{
			//if (editPost.Text == null && editPost.FilesToDelete.Count == 0 && editPost.NewFiles.Count == 0)
			//{
			//    return View(editPost);
			//}

			Log.Information("Попытка редактирования поста {postGuid} пользователем {user}", editPost.Id, HttpContext.User.FindFirstValue("CpcmUserId"));
			if (ModelState.IsValid)
			{
				CpcmPost? post = null;
				try
				{
					post = await _context.CpcmPosts.Include(c => c.CpcmImages).Where(c => c.CpcmPostId == editPost.Id && c.CpcmGroupId==editPost.GroupId).FirstOrDefaultAsync();

				}
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при получении поста из базы данных", editPost.Id);
					return StatusCode(500);
				}
				if (post == null)
				{
					Log.Warning("Пост не найден", editPost.Id);
					return StatusCode(404);
				}
				if (post.CpcmIsDeleted)
				{
					Log.Warning("Пост удалён", editPost.Id);
					return StatusCode(404);
				}
				if (!await CheckOnlyGroupPrivelege("CpcmCanEditPost", true, editPost.GroupId) || post.CpcmPostBanned)
				{
					Log.Warning("Недостаточно прав для редактирования поста", editPost.Id, HttpContext.User.FindFirstValue("CpcmUserId"));
					return StatusCode(403);
				}
				if (post.CpcmImages.Count - editPost.FilesToDelete.Count + editPost.NewFiles.Count > 4)
				{
					Log.Warning("Превышено количество фотографий в посте", editPost.Id);
					ModelState.AddModelError("NewFiles", "В посте не может быть больше 4 фотографий");
					return View(editPost);
				}

				post.CpcmPostText = editPost.Text.Trim();

				if (editPost.NewPublishDate == null)
				{
					post.CpcmPostPublishedDate = DateTime.UtcNow;
				}
				else
				{
					post.CpcmPostPublishedDate = editPost.NewPublishDate;
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								post.CpcmPostPublishedDate += offset;

							}
						}
					}
				}

				//post.CpcmPostPublishedDate = DateTime.UtcNow;
				if (post.CpcmPostFather != null)
				{
					editPost.FilesToDelete = new List<Guid>();
					editPost.NewFiles = new();
				}

				var imageLast = post.CpcmImages.Where(c => c.CpcmPostId == editPost.Id).OrderBy(k => k.CpcmImageOrder).LastOrDefault();
				int i = 0;
				if (imageLast != null)
				{
					i = imageLast.CpcmImageOrder;
				}
				List<string> filePaths = new List<string>();
				foreach (var file in editPost.NewFiles)
				{
					CheckIFormFile("NewFiles", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

					if (!ModelState.IsValid)
					{
						return View(editPost);

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

						post.CpcmImages.Add(image);
					}
					catch (IOException ex)
					{
						Log.Error("Ошибка при сохранении файла", editPost);
						try
						{
							foreach (var uploadedfile in filePaths)
							{
								if (System.IO.File.Exists(uploadedfile))
								{
									System.IO.File.Delete(uploadedfile);
								}
							}
						}
						catch (IOException exx)
						{
							Log.Error(exx, "Ошибка при удалении файла", editPost);
						}
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
						return View(editPost);
					}

				}


				List<CpcmImage>? images = post.CpcmImages.Where(c => !editPost.FilesToDelete.Contains(c.CpcmImageId)).ToList(); //TODO возможно ! тут не нужен 
				if (images != null || images.Count != 0)
				{
					//_context.CpcmImages.RemoveRange(images);
					foreach (var item in images)
					{
						post.CpcmImages.Remove(item);
					}

					try
					{
						await _context.SaveChangesAsync();

						var imagesAfterDeletion = post.CpcmImages.Where(c => c.CpcmPostId == post.CpcmPostId).OrderBy(i => i.CpcmImageOrder).ToList();

						for (int j = 0; j < imagesAfterDeletion.Count; j++)
						{
							imagesAfterDeletion[j].CpcmImageOrder = j;
						}
						i = imagesAfterDeletion.Last().CpcmImageOrder;


						try
						{
							foreach (var image in images)
							{
								if (System.IO.File.Exists(image.CpcmImagePath))
								{
									System.IO.File.Delete(image.CpcmImagePath);
								}
							}
						}
						catch (IOException ex)
						{
							Log.Error(ex, "Ошибка при удалении файла", editPost);
						}

					}
					catch (DbException ex)
					{

						try
						{
							foreach (var path in filePaths)
							{
								if (System.IO.File.Exists(path))
								{
									System.IO.File.Delete(path);
								}
							}
						}
						catch (IOException exx)
						{
							Log.Error(exx, "Ошибка при удалении файла", editPost);
						}
						Log.Error(ex, "Ошибка при сохранении изменений поста", editPost);
						return StatusCode(500);
					}
				}


				try
				{
					if (post.CpcmPostText == null && (await _context.CpcmImages.Where(p => p.CpcmPostId == post.CpcmPostId).ToListAsync()).Count == 0)
					{
						ModelState.AddModelError("Text", "Нельзя создать пустой пост");
						return View(editPost);
					}
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException ex)
				{
					Log.Error(ex, "Ошибка при сохранении изменений поста - конкурентность", editPost);
					ViewData["Message"] = "Обнаружена опытка редактировать пост, который кто-то редактировал";
					return View(editPost);
				}
				catch (DbUpdateException ex)
				{
					Log.Error(ex, "Ошибка при сохранении изменений поста", editPost);
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";

				}
				catch (DbException ex)
				{
					Log.Error(ex, "Ошибка при сохранении изменений поста", editPost);
					Response.StatusCode = 418;
					ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
					return View(editPost); // TODO Продумать место для сохранения еррора
				}

			}
			return View(editPost);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> GetNextNotPublishedPosts(Guid groupId, Guid lastPostId)
		{
			Log.Information("Попытка получения следующих неопубликованных постов группы {groupId} пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			List<CpcmPost> posts;
			ICollection<CpcmPost> postsWithLikesCount = new List<CpcmPost>();
			try
			{
				var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == lastPostId).FirstOrDefaultAsync();
				if (post == null)
				{
					Log.Warning("Пост не найден", lastPostId);
					return StatusCode(404);
				}

				if (!await CheckOnlyGroupPrivelege("CpcmCanMakePost", true, groupId))
				{
					Log.Warning("Недостаточно прав для получения постов", groupId,HttpContext.User.FindFirstValue("CpcmUserId"));
					return StatusCode(403);
				}
				long liked;
				posts = await _context.CpcmPosts.Where(c => c.CpcmGroupId == groupId).Where(c => c.CpcmPostPublishedDate < post.CpcmPostPublishedDate && c.CpcmPostPublishedDate > DateTime.UtcNow).Take(10).ToListAsync();

				foreach (var postik in posts)
				{
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
							return StatusCode(500);
						}
						if (liked > 0)
							postik.IsLiked = true;
						else
							postik.IsLiked = false;

					}
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								postik.CpcmPostPublishedDate -= offset;

							}
						}
					}
				}


			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении постов из базы данных", groupId);
				return StatusCode(500);
			}
			return PartialView(postsWithLikesCount);
		}


		[Authorize]
		[HttpGet]
		public async Task<IActionResult> NotPublishedPosts(Guid groupId)
		{
			Log.Information("Попытка получения неопубликованных постов группы {groupId} пользователем {user}", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
			if (!await CheckOnlyGroupPrivelege("CpcmCanMakePost", true, groupId))
			{
				Log.Warning("Недостаточно прав для получения неопубликованных постов", groupId, HttpContext.User.FindFirstValue("CpcmUserId"));
				return StatusCode(403);
			}

			List<CpcmPost> posts;
			//ICollection<CpcmPost> postsWithLikesCount = new List<CpcmPost>();
			try
			{
				posts = await _context.CpcmPosts.Where(c => c.CpcmGroupId == groupId && c.CpcmPostPublishedDate > DateTime.UtcNow).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
				foreach (var postik in posts)
				{
					postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								postik.CpcmPostPublishedDate -= offset;

							}
						}
					}
				}
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении постов из неопубликованных базы данных", groupId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			return View(posts);
		}


		public async Task<IActionResult> CheckCreateNickName(string CpcmGroupNickName)
		{
			Log.Information("Попытка проверки nickname {CpcmGroupNickName}", CpcmGroupNickName);
			if (CpcmGroupNickName == null || CpcmGroupNickName.All(char.IsWhiteSpace) || CpcmGroupNickName == string.Empty)
			{
				return Json(true);
			}
			CpcmGroupNickName = CpcmGroupNickName.Trim();
			if (CpcmGroupNickName.Contains("admin") || CpcmGroupNickName.Contains("webmaster") || CpcmGroupNickName.Contains("abuse"))
			{
				Log.Warning("Попытка зарегистрировать зарезервированный nickname", CpcmGroupNickName);
				return Json(data: $"{CpcmGroupNickName} зарезервировано");
			}
			
			bool rez = false;
			try
			{
				rez = !await _context.CpcmGroups.AnyAsync(e => e.CpcmGroupNickName == CpcmGroupNickName);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при проверке nickname", CpcmGroupNickName);
				return Json(data: "Не удалось установить соединение с сервером");
			}
			if (!rez)
				return Json(data: "Данный nickname уже занят");
			return Json(rez);
		}
		public async Task<IActionResult> CheckCreateNickName(string CpcmGroupNickName, Guid GroupId)
		{
			Log.Information("Попытка проверки nickname {CpcmGroupNickName}", CpcmGroupNickName);
			if (CpcmGroupNickName == null || CpcmGroupNickName.All(char.IsWhiteSpace) || CpcmGroupNickName == string.Empty)
			{
				return Json(true);
			}
			CpcmGroupNickName = CpcmGroupNickName.Trim();
			var authFactor = bool.Parse(User.FindFirstValue("CpcmCanEditGroups"));
			if (CpcmGroupNickName.Contains("admin") || CpcmGroupNickName.Contains("webmaster") || CpcmGroupNickName.Contains("abuse") && !authFactor)
			{
				Log.Warning("Попытка зарегистрировать зарезервированный nickname", CpcmGroupNickName);
				return Json(data: $"{CpcmGroupNickName} зарезервировано");
			}

			bool rez = false;
			try
			{
				rez = !await _context.CpcmGroups.AnyAsync(e => e.CpcmGroupNickName == CpcmGroupNickName && e.CpcmGroupId!=GroupId);
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при проверке nickname", CpcmGroupNickName);
				return Json(data: "Не удалось установить соединение с сервером");
			}
			if (!rez)
				return Json(data: "Данный nickname уже занят");
			return Json(rez);
		}
		private async Task<CpcmPost?> GetFatherPostReccurent(CpcmPost cpcmPostFatherNavigation)
		{
			try
			{
				var father = await _context.CpcmPosts.Where(p => p.CpcmPostId == cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
				if (father != null)
				{
					father.CpcmPostFatherNavigation = await GetFatherPostReccurent(father);
					father.User = await _context.CpcmUsers.Where(p => p.CpcmUserId == father.CpcmUserId).FirstOrDefaultAsync();
					father.Group = await _context.CpcmGroups.Where(p => p.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
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
				}
				return father;
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Не удалось выгрузить родительские посты {fathrepostnavigation}", cpcmPostFatherNavigation);
				throw;
			}
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

		
		private async Task<bool> CheckOnlyGroupPrivelege(string propertyName, object propertyValue, Guid? groupId)
		{
			CpcmGroupfollower? follower;
			try
			{
				follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId.ToString() == User.FindFirstValue("CpcmUserId") && f.CpcmGroupId == groupId).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
				
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении роли пользователя", groupId);
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
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении роли пользователя", id);
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

		private async Task<bool> CheckUserPrivilege(string propertyName, object propertyValue, string userPropName, object userPropVal, Guid? groupId)
		{
			CpcmGroupfollower? follower;
			CpcmUser? user;
			Guid id = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
			try
			{
				follower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == id && f.CpcmGroupId==groupId).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
				user = await _context.CpcmUsers.Where(f => f.CpcmUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Ошибка при получении роли пользователя", groupId);
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
