using KeepPasswords.Data;
using KeepPasswords.Models;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.TextKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
            var key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();

            if (model.Count()>0)
            {                
                ViewBag.Selected = model.First().ItemId;
            }
            
            foreach(var item in model)
            {
                item.DecryptedTitle = EncryptorDecryptor.DecryptToPlainText(key.SecretPhrase, item.Title);
                if(item.Text!=null)
                {
                    item.DecryptedText = EncryptorDecryptor.DecryptToPlainText(key.SecretPhrase, item.Text);
                }
            }

            foreach(var item in model)
            {                
                item.FormattedTitle = new string(item.DecryptedTitle.Take(20).ToArray());
                if(item.DecryptedTitle.Length>20)
                {
                    item.FormattedTitle += "...";
                }
                if(!String.IsNullOrEmpty(item.DecryptedText))
                {
                    item.TextWithoutHTML = new string(Regex.Replace(item.DecryptedText, "<[^>]+>", string.Empty).Take(20).ToArray());
                    if (item.DecryptedText.Length > 20)
                    {
                        item.TextWithoutHTML += "...";
                    }
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
                var key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();

                if (key == null)
                {
                    return new EmptyResult();
                }

                NoticeItem model = new NoticeItem();
                model.UserId = user.Id;
                model.Title = EncryptorDecryptor.EncryptPlainText(key.SecretPhrase, "Новая заметка"); 
                await context.UserNotices.AddAsync(model);
                await context.SaveChangesAsync();
                return Content(model.ItemId.ToString());
            }
            catch(Exception ex)
            {
                return new EmptyResult();
            }            
        }

        public async Task<IActionResult> ShowNote(int ItemId)
        {
            var user = await userManager.GetUserAsync(User);
            var model = context.UserNotices.Where(x=>x.UserId.Equals(user.Id) && x.ItemId == ItemId).FirstOrDefault();
            var key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();
            if(model!=null)
            {
                model.DecryptedTitle = EncryptorDecryptor.DecryptToPlainText(key.SecretPhrase, model.Title);
                if (model.Text != null)
                {
                    model.DecryptedText = EncryptorDecryptor.DecryptToPlainText(key.SecretPhrase, model.Text);
                }
            }
            
            return PartialView("NoticePartial", model);
        }

        public async Task<IActionResult> ShowModalDeleteNotice(int ItemId)
        {
            return PartialView("ModalDeleteConfirmation", ItemId);
        }

        public async Task<IActionResult> DeleteNotice(int ItemId)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var model = context.UserNotices.Where(x => x.UserId.Equals(user.Id) && x.ItemId == ItemId).FirstOrDefault();
                if(model==null)
                {
                    return Content("Текстовая заметка не найдена или не принадлежит пользователю!");
                }
                context.UserNotices.Remove(model);
                await context.SaveChangesAsync();
                return new EmptyResult();
            }
            catch(Exception ex) 
            {
                return Content(ex.Message);
            }
        }
        public async Task<IActionResult> UpdateNotice([Bind] NoticeItem model)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();

                if (key == null)
                {
                    return Content("Возникла ошибка с Secret Phrase!");
                }
                var existedModel = context.UserNotices.Where(x => x.ItemId == model.ItemId && x.UserId.Equals(user.Id)).FirstOrDefault();

                if (existedModel == null)
                {
                    return Content("Повторите попытку позже!");
                }

                existedModel.Title = EncryptorDecryptor.EncryptPlainText(key.SecretPhrase, model.Title);
                existedModel.Text = EncryptorDecryptor.EncryptPlainText(key.SecretPhrase, model.Text);
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
