using KeepPasswords.Data;
using KeepPasswords.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace KeepPasswords.Controllers.PasswordsKeeper
{
    public class PasswordsKeeperController : Controller
    {
        private readonly ApplicationContext context;
        private readonly UserManager<User> userManager;
        public PasswordsKeeperController(ApplicationContext context, UserManager<User> userManager) { 
            this.context = context;
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            bool hasSecretPhrase = context.UserSecretPhrases.Where(x=>x.UserId.Equals(user.Id)).Count() > 0 ? true : false;
            ViewBag.HasSecretPhrase = hasSecretPhrase;
            return View();
        }
        public async Task<IActionResult> GetPasswords()
        {
            return PartialView("PasswordsTablePartial");
        }

        public async Task<IActionResult> ShowModalCreateNewPassword()
        {
            return PartialView("ModalCreatePassword");
        }
    }
}
