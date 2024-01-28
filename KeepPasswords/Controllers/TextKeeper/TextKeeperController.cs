using KeepPasswords.Data;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.TextKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KeepPasswords.Controllers.TextKeeper
{
    [Authorize]
    public class TextKeeperController : Controller
    {
        private readonly ApplicationContext context;
        private readonly UserManager<User> userManager;
        public TextKeeperController(ApplicationContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            bool hasSecretPhrase = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).Count() > 0 ? true : false;
            ViewBag.HasSecretPhrase = hasSecretPhrase;
            return View();
        }

        public async Task<IActionResult> GetNoticeList()
        {
            var user = await userManager.GetUserAsync(User);
            var model = context.UserNotices.Where(x => x.UserId.Equals(user.Id)).ToList();
            return PartialView("NoticeListPartial", model);
        }

        public async Task<IActionResult> CreateNewNote()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                NoticeItem model = new NoticeItem();
                model.UserId = user.Id;
                model.Title = "Новая заметка";
                await context.UserNotices.AddAsync(model);
                await context.SaveChangesAsync();
                return Content(model.ItemId.ToString());
            }
            catch(Exception ex)
            {
                return Content(ex.Message.ToString());
            }            
        }

        public async Task<IActionResult> ShowNote(int ItemId)
        {
            var user = await userManager.GetUserAsync(User);
            var model = context.UserNotices.Where(x=>x.UserId.Equals(user.Id) && x.ItemId == ItemId).FirstOrDefault();
            return PartialView("NoticePartial", model);
        }
    }
}
