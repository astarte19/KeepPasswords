using KeepPasswords.Data;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.TextKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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

            if(model.Count()>0)
            {                
                ViewBag.Selected = model.First().ItemId;
            }
            
            foreach(var item in model)
            {                
                item.FormattedTitle = new string(item.Title.Take(20).ToArray());
                if(item.Title.Length>20)
                {
                    item.FormattedTitle += "...";
                }
                if(!String.IsNullOrEmpty(item.Text))
                {
                    item.TextWithoutHTML = new string(Regex.Replace(item.Text, "<[^>]+>", string.Empty).Take(10).ToArray()) + "...";
                }
               
            }

            if (!String.IsNullOrEmpty(HttpContext.Request.Cookies["TextKeeperSelectedNote"]))
            {
                ViewBag.Selected = Convert.ToInt32(HttpContext.Request.Cookies["TextKeeperSelectedNote"]);
            }
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

        public async Task<IActionResult> UpdateNotice([Bind] NoticeItem model)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var existedModel = context.UserNotices.Where(x => x.ItemId == model.ItemId && x.UserId.Equals(user.Id)).FirstOrDefault();

                if (existedModel == null)
                {
                    return Content("Повторите попытку позже!");
                }

                existedModel.Title = model.Title;
                existedModel.Text = model.Text;
                context.UserNotices.Update(existedModel);

                await context.SaveChangesAsync();
                return new EmptyResult();
            }
            catch(Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}
