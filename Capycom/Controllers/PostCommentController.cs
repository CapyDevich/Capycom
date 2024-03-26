using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Reflection.Metadata;

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
                CpcmPost? post = await _context.CpcmPosts.Where(p => p.CpcmPostId == postId).Include(p => p.CpcmImages).Include(p => p.CpcmPostFatherNavigation).FirstOrDefaultAsync();
                if(post == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пост не найден";
                    return View("UserError");
                }
                var topComments = await _context.CpcmComments.Where(p => p.CpcmPostId == post.CpcmPostId && p.CpcmCommentFather == null).Include(c => c.CpcmImages).Take(10).OrderBy(u => u.CpcmCommentCreationDate).ToListAsync(); // впринципе эту итерацию можно пихнуть сразу в тот метод
                foreach (var TopComment in topComments)
                {
                    TopComment.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(TopComment);
                }
                if(post.CpcmPostFatherNavigation != null)
                {
                    post.CpcmPostFatherNavigation.CpcmPostFatherNavigation = await GetFathrePostReccurent(post.CpcmPostFatherNavigation);
                }
                CpcmUser? userOwner = await _context.CpcmUsers.Where(u => u.CpcmUserId == post.CpcmUserId).FirstOrDefaultAsync();
                CpcmGroup? groupOwner = await _context.CpcmGroups.Where(u => u.CpcmGroupId == post.CpcmGroupId).FirstOrDefaultAsync();
                long likes = await _context.Database.ExecuteSqlInterpolatedAsync($@"SELECT COUNT(*) FROM CPCM_POSTLIKES WHERE CPCM_PostID = '{post.CpcmGroupId}'");
                long reposts = await _context.Database.ExecuteSqlInterpolatedAsync($@"SELECT COUNT(*) FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = '{post.CpcmGroupId}'");

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

        private async Task<CpcmPost?> GetFathrePostReccurent(CpcmPost cpcmPostFatherNavigation)
        {
            var father = await _context.CpcmPosts.Where(p => p.CpcmPostId== cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
            if(father != null)
            {
                father.CpcmPostFatherNavigation = await GetFathrePostReccurent(father);
            }
            return father;
        }

        private async Task<ICollection<CpcmComment>> GetCommentChildrenReccurent(CpcmComment? comm)
        {
            var children = await _context.CpcmComments.Where(c => c.CpcmCommentFather == comm.CpcmCommentId).Include(c => c.CpcmImages).ToListAsync();
            if(children.Count != 0)
            {
                foreach (var childComm in children)
                {
                    childComm.InverseCpcmCommentFatherNavigation = await GetCommentChildrenReccurent(childComm);
                }
            }
            return children;
        }

        [Authorize]

        public async Task<IActionResult> AddComment(CommentAddModel userComment)
        {
            if (ModelState.IsValid)
            {
                CpcmComment comment = new CpcmComment();
                comment.CpcmCommentId = Guid.NewGuid();
                comment.CpcmPostId = userComment.CpcmPostId;
                comment.CpcmCommentFather = userComment.CpcmCommentFather;
                comment.CpcmCommentText = userComment.CpcmCommentText;
                comment.CpcmCommentCreationDate = DateTime.UtcNow;

                List<string> filePaths = new List<string>();
                List<CpcmImage> images = new List<CpcmImage>();

                int i = 0;
                foreach (var file in userComment.Files)
                {
                    CheckIFormFile("Files", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (!ModelState.IsValid)
                    {
                        return StatusCode(400,new { message = "Неверный формат файла или превышен размер одного/нескольких файла"});

                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    filePaths.Add(Path.Combine("wwwroot", "uploads", uniqueFileName));

                    CpcmImage image = new CpcmImage();
                    image.CpcmImageId = Guid.NewGuid();
                    image.CpcmCommentId = comment.CpcmCommentId;
                    image.CpcmImagePath = filePaths.Last();
                    image.CpcmImageOrder = 0;
                    i++;

                    images.Add(image);


                    //Response.StatusCode = 500;
                    //ViewData["ErrorCode"] = 500;
                    //ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    //return StatusCode(500, new { message = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку" });

                }

                _context.AddRange(images);
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
                return StatusCode(200);
            }
            return StatusCode(400, new { message = "Текст не может быть пустым" });

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
                    return StatusCode(200);
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
                return StatusCode(200);

            }
            catch (DbException)
            {

                return StatusCode(500);
            }
        }
        public async Task<IActionResult> ViewComment(Guid commentId)
        {
            
            try
            {
                var comment = await _context.CpcmComments.Where(c => c.CpcmCommentId == commentId).FirstOrDefaultAsync();
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
        public async Task<IActionResult> GetNextComments(Guid postId, DateTime lastCommentDate)
        {
            try
            {
                var rez = await _context.CpcmComments.Where(c => c.CpcmCommentCreationDate.CompareTo(lastCommentDate) > 0 && c.CpcmPostId == postId && c.InverseCpcmCommentFatherNavigation == null).OrderBy(u => u.CpcmCommentCreationDate).Take(10).ToListAsync();
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
                return StatusCode(500);
            }
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
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "claimType" && c.Value == "claimValue");
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
