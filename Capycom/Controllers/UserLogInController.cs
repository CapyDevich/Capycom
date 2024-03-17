using Microsoft.AspNetCore.Mvc;
using Capycom.Models;
using Microsoft.Extensions.Options;
namespace Capycom.Controllers
{
    public class UserLogInController : Controller
    {
        private readonly CapycomContext _context;
        private readonly MyConfig _config;
        private readonly ILogger<HomeController> _logger;

        public UserLogInController(ILogger<HomeController> logger, CapycomContext context, IOptions<MyConfig> config)
        {
            _context = context;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> LogIn(UserLogInModel user)
        {
            if (ModelState.IsValid)
            {
                CpcmUser potentialUser = _context.CpcmUsers.Where(e => e.CpcmUserEmail == user.CpcmUserEmail).First();
                string potentialUserSalt = potentialUser.CpcmUserSalt;
                if (potentialUser.CpcmUserPwdHash == MyConfig.GetSha256Hash(user.CpcmUserPwd, potentialUserSalt, _config.ServerSol))
                {

                }
                else
                {
                    
                }
            }
            return View(user);
        }
    }
}
