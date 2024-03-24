using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Capycom.Models;
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
        public async Task<IActionResult> Index(Guid postId)
        {
            return View();
        }
        public async Task<IActionResult> AddComment(CommentAddModel comment)
        {
            return View();
        }
        public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId)
        {
            return View();
        }
        public async Task<IActionResult> ViewComment(Guid commentId)
        {
            return View();
        }
        public async Task<IActionResult> GetNextComments(Guid postId, DateTime lastCommentDate)
        {
            return View();
        }
    }
}
