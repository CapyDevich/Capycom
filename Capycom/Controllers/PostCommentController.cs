using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

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
            return View();
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
                if(comment == null)
                {
                    return StatusCode(404);
                }
                string authUserId = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value;
                if (authUserId == comment.CpcmUserId.ToString())
                {
                    comment.Deleted = true;
                    await _context.SaveChangesAsync();
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
            return View();
        }
        public async Task<IActionResult> GetNextComments(Guid postId, DateTime lastCommentDate)
        {
            return View();
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
