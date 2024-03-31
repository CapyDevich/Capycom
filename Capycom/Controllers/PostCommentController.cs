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
            try
            {
                CpcmPost? post = await _context.CpcmPosts.Where(p => p.CpcmPostId == postId).Include(p => p.CpcmImages).Include(p => p.CpcmPostFatherNavigation).ThenInclude(p => p.CpcmImages).FirstOrDefaultAsync();
                if(post == null || post.CpcmIsDeleted)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пост не найден";
                    return View("UserError");
                }
                if (post.CpcmPostBanned)
                {
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пост заблокирован";
                    return View("UserError");
                }
                var topComments = await _context.CpcmComments.Where(p => p.CpcmPostId == post.CpcmPostId && p.CpcmCommentFather == null).Include(c => c.CpcmImages).Include(c => c.CpcmUser).Take(10).OrderBy(u => u.CpcmCommentCreationDate).ToListAsync(); // впринципе эту итерацию можно пихнуть сразу в тот метод
                foreach (var TopComment in topComments)
                {
                    TopComment.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(TopComment);
                }
                if(post.CpcmPostFatherNavigation != null)
                {
                    post.CpcmPostFatherNavigation.CpcmPostFatherNavigation = await GetFatherPostReccurent(post.CpcmPostFatherNavigation);
                }
                CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
                long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();
                long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {post.CpcmPostId}").CountAsync();

                PostModel postModel = new() { Post=post,UserOwner=userOwner, GroupOwner = groupOwner, LikesCount=likes,RepostsCount=reposts, TopLevelComments=topComments};
                return View(post);
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Ошибка связи с сервером";
                return View("UserError");
            }
        }

        [Authorize]
        public async Task<IActionResult> AddComment(CommentAddModel userComment)
        {
            if (ModelState.IsValid)
            {
                if ((string.IsNullOrEmpty(userComment.CpcmCommentText) || string.IsNullOrWhiteSpace(userComment.CpcmCommentText)) && userComment.Files==null)
                {
                    return StatusCode(200, new { status = false, message = "Коммент не может быть пустым" });
                }
                CpcmComment comment = new CpcmComment();
                comment.CpcmCommentId = Guid.NewGuid();
                comment.CpcmPostId = userComment.CpcmPostId;
                comment.CpcmCommentFather = userComment.CpcmCommentFather;
                comment.CpcmCommentText = userComment.CpcmCommentText?.Trim();
                comment.CpcmCommentCreationDate = DateTime.UtcNow;
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
                catch (DbException)
                {
                    foreach (var file in filePaths)
                    {
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
                }
                catch (IOException)
                {
                    return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });
                }
                return StatusCode(200, new { status = true });
            }
            return StatusCode(200, new { status=false,message = "Комментарий имеет некорректные значения." });

        }

        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {

            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).FirstOrDefaultAsync();
                if (comment == null)
                {
                    return StatusCode(404);
                }
                string? authUserId = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value;
                if (authUserId == comment.CpcmUserId.ToString())
                {
                    comment.CpcmIsDeleted = true;
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
                return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
            }
        }

        [Authorize]
        public async Task<IActionResult> BanUnbanComment(Guid commentId)
        {
            if(!CheckUserPrivilege("CpcmCanDelUsersComments","True")){
                return StatusCode(403);
            }
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).FirstOrDefaultAsync();
                if(comment == null)
                {
                    return StatusCode(404);
                }
                comment.CpcmCommentBanned = !comment.CpcmCommentBanned;
                await _context.SaveChangesAsync();
                return StatusCode(200, new {status=true});

            }
            catch (DbException)
            {

                return StatusCode(500 , new {message = "Не удалось устиановить соединение с сервером"});
            }
        }
        public async Task<IActionResult> ViewComment(Guid commentId)
        {
            
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).Include(p => p.CpcmImages).Include(c => c.CpcmUser).FirstOrDefaultAsync();
                if (comment == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Коммент не найден";
                    return View("UserError");
                }
                comment.CpcmCommentBanned = !comment.CpcmCommentBanned;
                await _context.SaveChangesAsync();
                return View(comment);

            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
        }
        public async Task<IActionResult> GetNextComments(Guid postId, Guid lastCommentId)
        {
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
                }
                
                return Json(rez);
            }
            catch (DbException)
            {
                return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
            }
        }

        [Authorize]
        public async Task<IActionResult> AddRemoveLike(Guid postId)
        {
            try
            {
                var post = await _context.CpcmPosts.Where(p => p.CpcmPostId == postId && p.CpcmPostPublishedDate < DateTime.UtcNow).FirstOrDefaultAsync();
                if (post == null)
                {
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
            catch (DbException)
            {
                return StatusCode(500, new {message = "Не удалось установить соединение с сервером"});
            }
        }
        //public async Task<IActionResult> AddRepost(Guid postId)
        //{
        //    // перенести в контроллер юзера и там при создании поста если есть родитель - +
        //}

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
        private async Task<ICollection<CpcmComment>> GetCommentChildrenReccurent(CpcmComment? comm)
        {
            var children = await _context.CpcmComments.Where(c => c.CpcmCommentFather == comm.CpcmCommentId).Include(c => c.CpcmImages).Include(c=>c.CpcmUser).ToListAsync();
            if (children.Count != 0)
            {
                foreach (var childComm in children)
                {
                    childComm.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(childComm);
                }
            }
            return children;
        }

        public async Task<IActionResult> Test()
        {

            CpcmPost post = new CpcmPost();
            post.CpcmPostId = Guid.NewGuid();
            post.CpcmPostText = "sasd2";
            post.CpcmPostPublishedDate = DateTime.UtcNow;
            post.CpcmPostCreationDate = DateTime.UtcNow;
            post.CpcmUserId = Guid.Parse("EF3E530D-3CB2-4921-AB78-2F4DBD91A8C7");
            post.CpcmPostFather = Guid.Parse("1C689FCA-41CA-4AA4-B3B2-0E9A52B20042");
            _context.CpcmPosts.Add(post);
            _context.SaveChanges();

            //// CpcmComment com1 = new CpcmComment();
            //// com1.CpcmPostId = post.CpcmPostId;
            //// com1.CpcmCommentText = "1";
            //// com1.CpcmUserId = (Guid)post.CpcmUserId;
            //// com1.CpcmCommentId = Guid.NewGuid();
            //// com1.CpcmCommentCreationDate = DateTime.UtcNow;


            //// _context.CpcmComments.Add(com1);
            //// _context.CpcmComments.Add(com2);
            //// _context.SaveChanges();
            //// return Ok();

            //var post = _context.CpcmPosts.Find(Guid.Parse("1C689FCA-41CA-4AA4-B3B2-0E9A52B20042"));
            //CpcmComment com2 = new CpcmComment();
            //com2.CpcmPostId = post.CpcmPostId;
            //com2.CpcmCommentText = "2";
            //com2.CpcmUserId = Guid.Parse("EF3E530D-3CB2-4921-AB78-2F4DBD91A8C7");
            //com2.CpcmCommentFather = Guid.Parse("F52A5189-3708-41B0-8FB3-8BD5414DEBBC");
            //com2.CpcmCommentId = Guid.NewGuid();
            //com2.CpcmCommentCreationDate = DateTime.UtcNow;
            //_context.CpcmComments.Add(com2);
            //_context.SaveChanges();

            ////if (post?.CpcmComments != null)
            ////{
            ////    _context.Entry(post).Collection(p => p.CpcmComments).Load();
            ////}
            ////Console.Out.WriteLineAsync("123");

            //var com1 = _context.CpcmComments.Find(Guid.Parse("327372EF-0743-464B-B615-38FFFD3FB74F"));
            //_context.Entry(com1).Collection(p => p.InverseCpcmCommentFatherNavigation).Load();
            //Console.Out.WriteLineAsync("123");

            return StatusCode(200);



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
