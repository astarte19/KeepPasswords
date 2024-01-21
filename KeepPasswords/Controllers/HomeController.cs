using KeepPasswords.Data;
using KeepPasswords.Models;
using KeepPasswords.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KeepPasswords.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ShowModalError(string message)
        {
            return PartialView("ModalError", message);
        }
        public async Task<IActionResult> ShowModalSuccess(string message)
        {
            return PartialView("ModalSuccess", message);
        }
    }

    public class UserAvatarComponent: ViewComponent
    {
        private readonly ApplicationContext applicationContext;
        private readonly UserManager<User> _userManager;

        public UserAvatarComponent(ApplicationContext applicationContext, UserManager<User> _userManager)
        {
            this.applicationContext = applicationContext;
            this._userManager = _userManager;            
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var avatar = applicationContext.UserAvatars.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().Avatar;
            ViewBag.Avatar = avatar;
            return View("~/Views/Home/Components/UserAvatarComponent/UserAvatarComponent.cshtml");

        }

    }
}