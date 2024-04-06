﻿using Microsoft.AspNetCore.Mvc;
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
                if (post.CpcmPostBanned)
                {
                    Log.Information("Попытка просмотра заблокированного поста {Post}", postId);
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пост заблокирован";
                    return View("UserError");
                }
                var topComments = await _context.CpcmComments.Where(p => p.CpcmPostId == post.CpcmPostId && p.CpcmCommentFather == null).Include(c => c.CpcmImages).Include(c => c.CpcmUser).Take(10).OrderBy(u => u.CpcmCommentCreationDate).ToListAsync(); // впринципе эту итерацию можно пихнуть сразу в тот метод
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
            catch (DbException)
            {
                Log.Error("Не удалось выполнить запрос к БД на выборку поста {Post}", postId);
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Ошибка связи с сервером";
                return View("UserError");
            }
        }

        [Authorize]
        public async Task<IActionResult> AddComment(CommentAddModel userComment)
        {
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
                catch (IOException)
                {
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
                    return StatusCode(404);
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
                    return StatusCode(403);
                }
            }
			catch (DbUpdateConcurrencyException ex)
			{
				// Обработка исключений DbUpdateConcurrencyException
                Log.Error(ex, "Пользователь {User} пытается удалить комментарий {Comment}. Произошла ошибка с доступом к серверу - не кто-то уже работал с этим комментарием", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Пороверьте действительно ли существует комментарий, возможно вы ранее его уже удалили." });
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
            if(!CheckUserPrivilege("CpcmCanDelUsersComments","True")){
                Log.Warning("Пользователь {User} пытается заблокировать/разблокировать комментарий {Comment}. У пользователя недостаточно прав", HttpContext.User.FindFirstValue("CpcmUserId"), commentId);
                return StatusCode(403);
            }
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).FirstOrDefaultAsync();
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
                return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Пороверьте действительно ли существует комментарий, возможно вы ранее его уже заблокировали/разблокировали." });
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
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).Include(p => p.CpcmImages).Include(c => c.CpcmUser).FirstOrDefaultAsync();
                if (comment == null)
                {
                    Log.Warning("Попытка просмотра несуществующего комментария {Comment}", commentId);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Коммент не найден";
                    return View("UserError");
                }
                comment.CpcmCommentBanned = !comment.CpcmCommentBanned;
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
				await _context.SaveChangesAsync();
                return View(comment);

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
                if(lastComment == null) { return StatusCode(404); }
                var rez = await _context.CpcmComments.Where(c => c.CpcmCommentCreationDate.CompareTo(lastComment.CpcmCommentCreationDate) > 0 && c.CpcmPostId == postId && c.InverseCpcmCommentFatherNavigation == null).Include( c=>c.CpcmUser).OrderBy(u => u.CpcmCommentCreationDate).Take(10).ToListAsync();
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
                return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Пороверьте действительно ли существует пост, возможно вы ранее его уже лайкнули/дизлайкнули с другого утройства одновременно с этим устройством." });
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
                }
                return father;
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
            catch (DbException ex)
            {
                Log.Error(ex,"Не удалось выгрузить потомков комментария {@comm}",comm);
                throw;
            }
        }

        private bool CheckUserPrivilege(string claimType, string claimValue)
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
    }
}
