using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Reflection.Metadata;
using System.Security.Claims;
using Serilog;

namespace Capycom.Controllers
{
    public class PostCommentController : Controller
    {
        private readonly CapycomContext _context;
        private readonly MyConfig _config;
        private readonly ILogger<PostCommentController> _logger;

        public PostCommentController(ILogger<PostCommentController> logger, CapycomContext context, IOptions<MyConfig> config)
        {
            _context = context;
            _config = config.Value;
            _logger = logger;
        }
        public async Task<IActionResult> ViewPost(Guid postId)
        {
            if(User.Identity.IsAuthenticated)
                Log.Information("Пользователь {User} просматривает пост {Post}", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
            else
                Log.Information("Неавторизирвоанный клиент {@client} просматривает пост {Post}",HttpContext.Connection, postId);

            try
            {
                CpcmPost? post = await _context.CpcmPosts.Where(p => p.CpcmPostId == postId).Include(p => p.CpcmImages).Include(p => p.CpcmPostFatherNavigation).ThenInclude(p => p.CpcmImages).FirstOrDefaultAsync();
                if(post == null || post.CpcmIsDeleted)
                {
                    Log.Information("Попытка просмотра несуществующего поста {Post}", postId);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пост не найден";
                    return View("UserError");
                }
                post.User = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                post.Group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
                if (post.CpcmPostBanned)
                {
                    Log.Information("Попытка просмотра заблокированного поста {Post}", postId);
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пост заблокирован";
                    return View("UserError");
                }
                if (post.User == null || post.User.CpcmIsDeleted)
                {
					Log.Information("Попытка просмотра поста удалённого Юзера {Post}", postId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пост не найден";
					return View("UserError");
				}
                if (post.User != null && post.User.CpcmUserBanned)
                {
					Log.Information("Попытка просмотра поста заблокированного Юзера {Post}", postId);
					Response.StatusCode = 403;
					ViewData["ErrorCode"] = 403;
					ViewData["Message"] = "Автор поста заблокирован";
					return View("UserError");
				}
                if (post.User==null)
                {
                    if (post.Group == null || post.Group.CpcmIsDeleted)
                    {
                        Log.Information("Попытка просмотра поста удалённой Группы {Post}", postId);
                        Response.StatusCode = 404;
                        ViewData["ErrorCode"] = 404;
                        ViewData["Message"] = "Пост не найден";
                        return View("UserError");
                    }
                    if ((post.Group != null && post.Group.CpcmGroupBanned))
                    {
                        Log.Information("Попытка просмотра поста заблокированной Группы{Post}", postId);
                        Response.StatusCode = 403;
                        ViewData["ErrorCode"] = 403;
                        ViewData["Message"] = "Группа поста заблокирован";
                        return View("UserError");
                    } 
                }
                if(post.CpcmPostPublishedDate > DateTime.UtcNow)
                {
					Log.Information("Попытка просмотра отложенного поста {Post}", postId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пост не найден";
					return View("UserError");
				}
				var topComments = await _context.CpcmComments.Where(p => p.CpcmPostId == post.CpcmPostId && p.CpcmCommentFather == null && !p.CpcmIsDeleted).Include(c => c.CpcmImages).Include(c => c.CpcmUser).Take(10).OrderBy(u => u.CpcmCommentCreationDate).ToListAsync(); // впринципе эту итерацию можно пихнуть сразу в тот метод
                foreach (var TopComment in topComments)
                {
                    TopComment.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(TopComment);
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								TopComment.CpcmCommentCreationDate -= offset;

							}
						}
					}
				}
                if(post.CpcmPostFatherNavigation != null)
                {
                    post.CpcmPostFatherNavigation = await GetFatherPostReccurent(post);
                }
                CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
				if (User.Identity.IsAuthenticated && groupOwner != null )
				{
					var authUserId = GetUserIdString();
					var authFollower = await _context.CpcmGroupfollowers.Where(f => f.CpcmUserId == authUserId && f.CpcmGroupId == groupOwner.CpcmGroupId).Include(f => f.CpcmUserRoleNavigation).FirstOrDefaultAsync();
                    groupOwner.UserFollowerRole = authFollower.CpcmUserRoleNavigation;
                    post.Group.UserFollowerRole = authFollower.CpcmUserRoleNavigation;
				}
				long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();
                long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();

				if (User.Identity.IsAuthenticated)
				{
					long liked = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId} AND CPCM_UserID = {post.CpcmUserId}").CountAsync();
					if (liked > 0)
						post.IsLiked = true;
					else
						post.IsLiked = false;
				}

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


				PostModel postModel = new() { Post=post,UserOwner=userOwner, GroupOwner = groupOwner, LikesCount=likes,RepostsCount=reposts, TopLevelComments=topComments};
                return View(postModel);
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к БД на выборку поста {Post}", postId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
            catch (DbException ex )
            {
                Log.Error(ex,"Не удалось выполнить запрос к БД на выборку поста {Post}", postId);
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Ошибка связи с сервером";
                return View("UserError");
            }
        }

        [Authorize]
        public async Task<IActionResult> AddComment(CommentAddModel userComment)
        {
            try
            {
                var post = await _context.CpcmPosts.Where(p => p.CpcmPostId == userComment.CpcmPostId).FirstOrDefaultAsync();
                if(post == null || post.CpcmIsDeleted)
                {   
                    Log.Warning("Пользователь {User} пытается добавить комментарий к несуществующему посту {Post}", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
					return StatusCode(404);
				}
                post.User = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                post.Group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
                if (post.CpcmPostBanned)
                {
                    Log.Warning("Пользователь {User} пытается добавить комментарий к заблокированному посту {Post}", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
                    return StatusCode(403, new {message = "Пост заблокирован"});
                }
                if(post.User==null || post.User.CpcmIsDeleted)
                {
                    Log.Warning("Пользователь {User} пытается добавить комментарий к посту {Post}. Автор поста удалён", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
					return StatusCode(404);
				}
                if (post.User != null && post.User.CpcmUserBanned)
                {
                    Log.Warning("Пользователь {User} пытается добавить комментарий к посту {Post}. Автор поста заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
                    return StatusCode(403, new { message = "Автор поста заблокирован" });
				
                }
                if (post.User==null)
                {
                    if (post.Group == null || post.Group.CpcmIsDeleted)
                    {
                        Log.Warning("Пользователь {User} пытается добавить комментарий к посту {Post}. Группа поста удалена", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
                        return StatusCode(404);
                    }
                    if (post.Group != null && post.Group.CpcmGroupBanned)
                    {
                        Log.Warning("Пользователь {User} пытается добавить комментарий к посту {Post}. Группа поста заблокирована", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
                        return StatusCode(403, new { message = "Группа поста заблокирована" });
                    } 
                }
				if (post.CpcmPostPublishedDate > DateTime.UtcNow)
				{
					Log.Information("Попытка просмкомментировать отложенный пост {Post}", post.CpcmPostId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пост не найден";
					return View("UserError");
				}
			}
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к БД на выборку поста {Post}", userComment.CpcmPostId);
                return StatusCode(500, new { message = "Ошибка связи с сервером" });

            }
            catch(DbException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к БД на выборку поста {Post}", userComment.CpcmPostId);
				return StatusCode(500, new { message = "Ошибка связи с сервером" });
			}
            Log.Information("Пользователь {User} пытается добавить комментарий к посту {Post}", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
            if (ModelState.IsValid)
            {
                if ((string.IsNullOrEmpty(userComment.CpcmCommentText) || string.IsNullOrWhiteSpace(userComment.CpcmCommentText)) && userComment.Files==null)
                {
                    Log.Warning("Пользователь {User} пытается добавить комментарий к посту {Post}. Коммент не может быть пустым", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
                    return StatusCode(200, new { status = false, message = "Коммент не может быть пустым" });
                }
                CpcmComment comment = new CpcmComment();
                comment.CpcmCommentId = Guid.NewGuid();
                comment.CpcmPostId = userComment.CpcmPostId;
                comment.CpcmCommentFather = userComment.CpcmCommentFather;
                comment.CpcmCommentText = userComment.CpcmCommentText?.Trim();
                comment.CpcmCommentCreationDate = DateTime.UtcNow;
				if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
				{
					string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
					if (timezoneOffsetCookie != null)
					{
						if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
						{
							TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

							comment.CpcmCommentCreationDate += offset;

						}
					}
				}
				comment.CpcmUserId = Guid.Parse(User.FindFirstValue("CpcmUserId"));

                List<string> filePaths = new List<string>();
                List<CpcmImage> images = new List<CpcmImage>();

                if (userComment.Files !=null)
                {
                    int i = 0;
                    foreach (var file in userComment.Files)
                    {
                        CheckIFormFile("Files", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                        if (!ModelState.IsValid)
                        {
                            return StatusCode(200, new { status=false,message = "Неверный формат файла или превышен размер одного/нескольких файла" });

                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        filePaths.Add(Path.Combine("wwwroot", "uploads", uniqueFileName));

                        CpcmImage image = new CpcmImage();
                        image.CpcmImageId = Guid.NewGuid();
                        image.CpcmCommentId = comment.CpcmCommentId;
                        image.CpcmImagePath = filePaths.Last().Replace("wwwroot","");
                        image.CpcmImageOrder = 0;
                        i++;

                        images.Add(image);


                        //Response.StatusCode = 500;
                        //ViewData["ErrorCode"] = 500;
                        //ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                        //return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });

                    }

                    _context.AddRange(images); 
                }
                _context.Add(comment);
                try
                {
                    for (int j = 0; j < filePaths.Count; j++)
                    {
                        using (var fileStream = new FileStream(filePaths[j], FileMode.Create))
                        {
                            await userComment.Files[j].CopyToAsync(fileStream);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbException ex)
                {
                    Log.Error(ex, "Пользователь {User} пытается добавить комментарий к посту {Post}. Произошла ошибка с доступом к серверу - не удалось выполнить запрос. Comment - {@comment}", HttpContext.User.FindFirstValue("CpcmUserId"),comment.CpcmPostId, comment);
                    foreach (var file in filePaths)
                    {
                        try
                        {
                            if (System.IO.File.Exists(file))
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                        catch (IOException exx)
                        {
                            Log.Error(exx, "Пользователь {User} пытается добавить комментарий к посту {Post}. Произошла ошибка с доступом к серверу - не удалось удалить файлы {@filePaths}", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId, filePaths);
							return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
						}
                    }
                    return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
                }
                catch (IOException ex)
                {
                    Log.Error(ex, "Пользователь {User} пытается добавить комментарий к посту {Post}. Произошла ошибка с доступом к серверу - не удалось создать файлы", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId);
                    return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
                }
                //return StatusCode(200, new { status = true });
                return PartialView(comment.CpcmImages = images);
            }
            Log.Information("Пользователь {User} пытается добавить комментарий к посту {Post}. Комментарий имеет некорректные значения , {@Comment}", HttpContext.User.FindFirstValue("CpcmUserId"), userComment.CpcmPostId, userComment);
            return StatusCode(200, new { status=false,message = "Комментарий имеет некорректные значения.",errors= ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList() });

        }

        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
			Log.Information("Пользователь {User} пытается удалить комментарий {Comment}", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId && c.CpcmIsDeleted==false).FirstOrDefaultAsync();
                if (comment == null)
                {
                    Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Коммент не найден или удалён", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                    return StatusCode(404);
                }



				var post = await _context.CpcmPosts.Where(p => p.CpcmPostId == comment.CpcmPostId).FirstOrDefaultAsync();
				if (post == null || post.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Пост не найден или удалён", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
					return StatusCode(404);
				}
                post.User = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                post.Group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
				if (post.CpcmPostBanned)
				{
                    Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Пост заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
					return StatusCode(403, new { message = "Пост заблокирован" });
				}
				if (post.User == null || post.User.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Автор поста удалён", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
					return StatusCode(404);
				}
				if (post.User != null && post.User.CpcmUserBanned)
				{
                    Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Автор поста заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
					return StatusCode(403, new { message = "Автор поста заблокирован" });

				}
                if (post.User==null)
                {
                    if (post.Group == null || post.Group.CpcmIsDeleted)
                    {
                        Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Группа поста удалена", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                        return StatusCode(404);
                    }
                    if (post.Group != null || post.Group.CpcmGroupBanned)
                    {
                        Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. Группа поста заблокирована", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                        return StatusCode(403, new { message = "Группа поста заблокирована" });
                    } 
                }



				string? authUserId = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value;
                if (authUserId == comment.CpcmUserId.ToString())
                {
                    comment.CpcmIsDeleted = true;
                    await _context.SaveChangesAsync();
                    Log.Information("Пользователь {User} удалил комментарий {Comment}", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                    return StatusCode(200, new { status = true });
                }
                else
                {
                    Log.Warning("Пользователь {User} пытается удалить комментарий {Comment}. У пользователя недостаточно прав", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                    return StatusCode(403);
                }
            }
			catch (DbUpdateConcurrencyException ex)
			{
				// Обработка исключений DbUpdateConcurrencyException
                Log.Error(ex, "Пользователь {User} пытается удалить комментарий {Comment}. Произошла ошибка с доступом к серверу - не кто-то уже работал с этим комментарием", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(409, new { message = "Произошла ошибка с доступом к серверу. Пороверьте действительно ли существует комментарий, возможно вы ранее его уже удалили." });
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Пользователь {User} пытается удалить комментарий {Comment}. Произошла ошибка с доступом к серверу - не удалось выполнить запрос", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
			}
			catch (DbException)
            {
                return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
            }
        }

        [Authorize]
        public async Task<IActionResult> BanUnbanComment(Guid commentId)
        {
            Log.Information("Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
            if(!CheckUserAdminPrivilege("CpcmCanDelUsersComments","True")){
                Log.Warning("Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}. У пользователя недостаточно прав", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(403);
            }
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId && c.CpcmIsDeleted != true).FirstOrDefaultAsync();
                if(comment == null)
                {
                    Log.Warning("Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}. Коммент не найден или удалён", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                    return StatusCode(404);
                }
                comment.CpcmCommentBanned = !comment.CpcmCommentBanned;
                await _context.SaveChangesAsync();
                return StatusCode(200, new {status=true});

            }
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}. Произошла ошибка с доступом к серверу - кто-то уже работал с этим комментарием", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(409, new { message = "Произошла ошибка с доступом к серверу. Пороверьте действительно ли существует комментарий, возможно вы ранее его уже заблокировали/разблокировали." });
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}. Произошла ошибка с доступом к серверу - не удалось выполнить запрос", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
				return StatusCode(500, new { message = "Не удалось отредактировать комментарий. Перезагрузите страницу и повторите" });
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}. Произошла ошибка с доступом к серверу. Произошла общая ошибка", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(500 , new {message = "Не удалось устиановить соединение с сервером"});
            }
        }
        public async Task<IActionResult> ViewComment(Guid commentId)
        {
            if(User.Identity.IsAuthenticated)
                Log.Information("Пользователь {User} просматривает комментарий {Comment}", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
            else
                Log.Information("Неавторизирвоанный клиент {@client} просматривает комментарий {Comment}",HttpContext.Connection, commentId);
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).Include(p => p.CpcmImages).Include(c => c.CpcmUser)
                    .Include(c => c.CpcmPost).Include(c => c.CpcmPost).FirstOrDefaultAsync();
                if (comment == null)
                {
                    Log.Warning("Попытка просмотра несуществующего комментария {Comment}", commentId);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Коммент не найден";
                    return View("UserError");
                }
                comment.CpcmPost.User = await _context.CpcmUsers.Where(u => u.CpcmUserId == comment.CpcmPost.CpcmUserId).FirstOrDefaultAsync();
                comment.CpcmPost.Group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == comment.CpcmPost.CpcmGroupId).FirstOrDefaultAsync();
                if (comment.CpcmUser.CpcmIsDeleted)
                {
					Log.Warning("Попытка просмотра комментария удалённого пользователя{Comment}", commentId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Коммент не найден";
					return View("UserError");
				}
				if (comment.CpcmUser.CpcmUserBanned)
				{
					Log.Warning("Попытка просмотра комментария забаненного пользователя{Comment}", commentId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Коммент не найден";
					return View("UserError");
				}
                if(comment.CpcmPost.User == null || comment.CpcmPost.User.CpcmIsDeleted)
                {
                    Log.Warning("Попытка просмотра комментария к посту удалённого пользователя{Comment}", commentId);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Коммент не найден";
                    return View("UserError");
                }
                if(comment.CpcmPost.User != null && comment.CpcmPost.User.CpcmUserBanned)
                {
					Log.Warning("Попытка просмотра комментария к посту забаненного пользователя{Comment}", commentId);
					Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пост комментария принадлежит заблокированному пользователю";
					return View("UserError");
				}
                if (comment.CpcmPost.User==null)
                {
                    if (comment.CpcmPost.Group == null || comment.CpcmPost.Group.CpcmIsDeleted)
                    {
                        Log.Warning("Попытка просмотра комментария к посту удалённой группы{Comment}", commentId);
                        Response.StatusCode = 404;
                        ViewData["ErrorCode"] = 404;
                        ViewData["Message"] = "Коммент не найден";
                        return View("UserError");
                    }
                    if (comment.CpcmPost.Group != null && comment.CpcmPost.Group.CpcmGroupBanned)
                    {
                        Log.Warning("Попытка просмотра комментария к посту забаненной группы{Comment}", commentId);
                        Response.StatusCode = 403;
                        ViewData["ErrorCode"] = 403;
                        ViewData["Message"] = "Пост комментария принадлежит заблокированной группе";
                        return View("UserError");
                    } 
                }
				//comment.CpcmCommentBanned = !comment.CpcmCommentBanned;
				if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
				{
					string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
					if (timezoneOffsetCookie != null)
					{
						if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
						{
							TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

							comment.CpcmCommentCreationDate -= offset;

						}
					}
				}
				if (comment.CpcmPost.CpcmPostPublishedDate > DateTime.UtcNow)
				{
					Log.Information("Попытка просмотра коммента ??? отложенного поста {Post}", comment.CpcmPost.CpcmPostId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пост не найден";
					return View("UserError");
				}
				await _context.SaveChangesAsync();
                return View(comment);

            }
            catch(DbUpdateException ex)
            {
				Log.Warning(ex, "Не удалось выполнить запрос к БД на выборку комментария {Comment}. Произошла общая ошибка.", commentId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Warning(ex, "Не удалось выполнить запрос к БД на выборку комментария {Comment}. Произошла общая ошибка.", commentId);
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
        }
        public async Task<IActionResult> GetNextComments(Guid postId, Guid lastCommentId)
        {
			if (User.Identity.IsAuthenticated)
                Log.Information("Пользователь {User} запрашивает следующие комментарии к посту {Post}", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
            else
                Log.Information("Неавторизирвоанный клиент {@client} запрашивает следующие комментарии к посту {Post}", HttpContext.Connection, postId);

			try
            {
                var lastComment = await _context.CpcmComments.Where(c => c.CpcmCommentId == lastCommentId).FirstOrDefaultAsync();
                if(lastComment == null) 
                {
                    Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Последний отрендеренный комментарий не найден", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                    return StatusCode(404); 
                }


				var post = await _context.CpcmPosts.Where(p => p.CpcmPostId == lastComment.CpcmPostId).FirstOrDefaultAsync();
				if (post == null || post.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Пост не найден", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(404);
				}
                post.User = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                post.Group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
				if (post.CpcmPostBanned)
				{
                    Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Пост заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(403, new { message = "Пост заблокирован" });
				}
				if (post.User == null || post.User.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Автор поста удалён", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(404);
				}
				if (post.User != null || post.User.CpcmUserBanned)
				{
                    Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Автор поста заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(403, new { message = "Автор поста заблокирован" });

				}
                if (post.User==null)
                {
                    if (post.Group == null || post.Group.CpcmIsDeleted)
                    {
                        Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Группа поста удалена", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                        return StatusCode(404);
                    }
                    if (post.Group != null || post.Group.CpcmGroupBanned)
                    {
                        Log.Warning("Пользователь {User} запрашивает следующие комментарии к посту {Post}. Группа поста заблокирована", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                        return StatusCode(403, new { message = "Группа поста заблокирована" });
                    } 
                }
				if (post.CpcmPostPublishedDate > DateTime.UtcNow)
				{
					Log.Information("Попытка просмотра коммента ??? отложенного поста {Post}", post.CpcmPostId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пост не найден";
					return View("UserError");
				}




				var rez = await _context.CpcmComments.Where(c => c.CpcmCommentCreationDate > lastComment.CpcmCommentCreationDate && c.CpcmPostId == postId && c.InverseCpcmCommentFatherNavigation == null).Include( c=>c.CpcmUser).OrderBy(u => u.CpcmCommentCreationDate).Take(10).ToListAsync();
                foreach (var comment in rez)
                {
                    //await _context.Entry(comment).Collection(p => p.InverseCpcmCommentFatherNavigation).LoadAsync();
                    //await _context.Entry(comment).Reference(p => p.CpcmCommentFatherNavigation).LoadAsync();
                    comment.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(comment);
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								comment.CpcmCommentCreationDate -= offset;

							}
						}
					}
				}
                
                return PartialView(rez);
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к БД на выборку комментариев к посту {Post}. Произошла общая ошибка", postId);
				return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к БД на выборку комментариев к посту {Post}. Произошла общая ошибка", postId);
                return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
            }
        }

        [Authorize]
        public async Task<IActionResult> AddRemoveLike(Guid postId)
        {
            Log.Information("Пользователь {User} пытается поставить/убрать лайк к посту {Post}", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
            try
            {
                var post = await _context.CpcmPosts.Where(p => p.CpcmPostId == postId && p.CpcmPostPublishedDate < DateTime.UtcNow).FirstOrDefaultAsync();
                if (post == null)
                {
                    Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Пост не найден", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                    return StatusCode(404);
                }
                post.User = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                post.Group = await _context.CpcmGroups.Where(g => g.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();

				if (post == null || post.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Пост не найден или удалён", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(404);
				}
				if (post.CpcmPostBanned)
				{
                    Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Пост заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(403, new { message = "Пост заблокирован" });
				}
				if (post.User == null || post.User.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Автор поста удалён", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(404);
				}
				if (post.User != null && post.User.CpcmUserBanned)
				{
                    Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Автор поста заблокирован", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
					return StatusCode(403, new { message = "Автор поста заблокирован" });

				}
                if (post.User==null)
                {
                    if (post.Group == null || post.Group.CpcmIsDeleted)
                    {
                        Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Группа поста удалена", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                        return StatusCode(404);
                    }
                    if (post.Group != null && post.Group.CpcmGroupBanned)
                    {
                        Log.Warning("Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Группа поста заблокирована", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                        return StatusCode(403, new { message = "Группа поста заблокирована" });
                    } 
                }
				if (post.CpcmPostPublishedDate > DateTime.UtcNow)
				{
					Log.Information("Попытка лайка отложенного поста {Post}", post.CpcmPostId);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пост не найден";
					return View("UserError");
				}





				string? userId = HttpContext.User.FindFirstValue("CpcmUserId");
                var answer = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId} AND CPCM_UserId = {userId} ").CountAsync();
                
                if(answer == 0)
                {
                    var querry = await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO CPCM_POSTLIKES VALUES ({post.CpcmPostId},{userId})");
                    if(querry ==1)
                    {
                        return StatusCode(200, new { status=true});
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
                    }
                }
                else
                {
                    // удаление лайка
                    var querry = await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId} AND CPCM_UserId = {userId} ");
                    if (querry == 1)
                    {
                        return StatusCode(200, new { status = true });
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
                    }

                }

            }
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Произошла ошибка с доступом к серверу - кто-то уже работал с этим постом", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                return StatusCode(409, new { message = "Произошла ошибка с доступом к серверу. Пороверьте действительно ли существует пост, возможно вы ранее его уже лайкнули/дизлайкнули с другого утройства одновременно с этим устройством." });
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Произошла ошибка с доступом к серверу - не удалось выполнить запрос", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
				return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Пользователь {User} пытается поставить/убрать лайк к посту {Post}. Произошла общая ошибка с доступом к серверу", HttpContext.User.FindFirstValue("CpcmUserId"), postId);
                return StatusCode(500, new {message = "Не удалось установить соединение с сервером"});
            }
        }
        //public async Task<IActionResult> AddRepost(Guid postId)
        //{
        //    // перенести в контроллер юзера и там при создании поста если есть родитель - +
        //}

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
					var author = await _context.CpcmUsers.Where(u => u.CpcmUserId == father.CpcmUserId).FirstOrDefaultAsync();
					if (author == null)
					{
						var group = await _context.CpcmGroups.Where(u => u.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
						father.Group = group;
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
        private async Task<ICollection<CpcmComment>> GetCommentChildrenReccurent(CpcmComment? comm)
        {
            try
            {
                var children = await _context.CpcmComments.Where(c => c.CpcmCommentFather == comm.CpcmCommentId).Include(c => c.CpcmImages).Include(c => c.CpcmUser).ToListAsync();
                if (children.Count != 0)
                {
                    foreach (var childComm in children)
                    {
                        childComm.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(childComm);
                        if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
                        {
                            string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
                            if (timezoneOffsetCookie != null)
                            {
                                if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
                                {
                                    TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

                                    childComm.CpcmCommentCreationDate -= offset;

                                }
                            }
                        }
                    }
                }
                return children;
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выгрузить потомков комментария {@comm}", comm);
				throw;
			}
            catch (DbException ex)
            {
                Log.Error(ex,"Не удалось выгрузить потомков комментария {@comm}",comm);
                throw;
            }
        }

        private bool CheckUserAdminPrivilege(string claimType, string claimValue)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
                return false;
            }
            return true;
        }
        private bool CheckIFormFileContent(IFormFile cpcmUserImage, string[] permittedTypes)//TODO: Объединить с методами при регистрации
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
        private bool CheckIFormFileSize(IFormFile cpcmUserImage, int size)//TODO: Объединить с методами при регистрации
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
        private bool CheckIFormFile(string FormFieldName, IFormFile file, int size, string[] permittedTypes)//TODO: Объединить с методами при регистрации
        {
            bool status = true;
            if (!CheckIFormFileContent(file, permittedTypes))
            {
                Log.Warning("Пользователь {User} пытается загрузить файл с недопустимым форматом {@file}", HttpContext.User.FindFirstValue("CpcmUserId"), file);
                ModelState.AddModelError(FormFieldName, "Допустимые типы файлов: png, jpeg, jpg, gif");
                status = false;
            }
            if (!CheckIFormFileSize(file, size))
            {
                Log.Warning("Пользователь {User} пытается загрузить файл с превышенным размером {@file}", HttpContext.User.FindFirstValue("CpcmUserId"), file);
                ModelState.AddModelError(FormFieldName, $"Максимальный размер файла: {size / 1024} Кбайт");
                status = false;
            }
            return status;
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
