using HtmlAgilityPack;
using KeepPasswords.Data;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.PasswordsKeeper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Policy;
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.RegularExpressions;
using KeepPasswords.Models;

namespace KeepPasswords.Controllers.PasswordsKeeper
{
    public class PasswordsKeeperController : Controller
    {
        private readonly ApplicationContext context;
        private readonly UserManager<User> userManager;
        public PasswordsKeeperController(ApplicationContext context, UserManager<User> userManager)
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
        public async Task<IActionResult> GetPasswords()
        {
            var user = await userManager.GetUserAsync(User);
            var model = context.UserPasswordManager.Where(x => x.UserId.Equals(user.Id)).ToList();
            foreach (var item in model)
            {
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString(item.WebSite);
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlCode);

                item.IconURL = GetWebsiteIconUrl(htmlDocument, item.WebSite);
            }


            return PartialView("PasswordsTablePartial", model);
        }

        public async Task<IActionResult> ShowModalCreateNewPassword()
        {
            PasswordItem model = new PasswordItem();
            model.ItemId = 0;
            return PartialView("ModalAddUpdatePassword", model);
        }

        public async Task<IActionResult> ShowModalUpdatePassword(int ItemId)
        {
            var user = await userManager.GetUserAsync(User);
            var model = context.UserPasswordManager.Where(x => x.ItemId == ItemId && x.UserId.Equals(user.Id)).FirstOrDefault();            
            string key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().SecretPhrase;
            string decreptedPassword = EncryptorDecryptor.DecryptToPlainText(key,model.Password);
            model.Password = decreptedPassword;
            return PartialView("ModalAddUpdatePassword", model);
        }

        static string GetWebsiteIconUrl(HtmlDocument document, string websiteUrl)
        {
            // Поиск всех ссылок на иконки в метатегах
            var linkTags = document.DocumentNode.SelectNodes("//link[@rel='icon' or @rel='shortcut icon']");

            if (linkTags != null && linkTags.Count > 0)
            {
                string iconUrl = linkTags[0].GetAttributeValue("href", "");
                if (!string.IsNullOrWhiteSpace(iconUrl))
                {
                    // Если ссылка на иконку задана относительно, то преобразуем ее в абсолютную ссылку
                    if (!Uri.IsWellFormedUriString(iconUrl, UriKind.Absolute))
                    {
                        iconUrl = new Uri(new Uri(websiteUrl), iconUrl).AbsoluteUri;
                    }
                    return iconUrl;
                }
            }

            // Если иконка не найдена, возвращаем ссылку на стандартную иконку
            return ""; // замените на нужный URL
        }        

        public async Task<IActionResult> AddUpdateService([Bind] PasswordItem model)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var userSecretKey = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();

                if (userSecretKey == null)
                {
                    return Content("Не установлен Secret Phrase!");
                }

                if(String.IsNullOrEmpty(model.ServiceName) || model.ServiceName.Length < 3)
                {
                    return Content("Название сервиса должно иметь более 3 символов!");
                }

                if (String.IsNullOrEmpty(model.UserName) || model.UserName.Length < 3)
                {
                    return Content("Имя пользователя должно иметь более 3 символов!");
                }

                if(model.Email!=null)
                {
                    if (!IsEmailValid(model.Email))
                    {
                        return Content("Email введен некорректно!");
                    }
                }

                if (String.IsNullOrEmpty(model.Password) || model.Password.Length < 3)
                {
                    return Content("Пароль должен иметь более 5 символов!");
                }

                if (!IsUrlValid(model.WebSite))
                {
                    return Content("URL введена некорректно!");
                }

                model.UserId = user.Id;
                model.RecentDateChange = DateTime.Now;
               
                string key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().SecretPhrase;
                if (model.ItemId != 0)
                {
                    var existedModel = context.UserPasswordManager.Where(x => x.ItemId == model.ItemId && x.UserId.Equals(model.UserId)).FirstOrDefault();
                    if (existedModel == null)
                    {
                        return Content("Возникла ошибка при обновлении сервиса!");
                    }
                    else
                    {
                        existedModel.RecentDateChange = DateTime.Now;
                        existedModel.WebSite = model.WebSite;
                        existedModel.ServiceName = model.ServiceName;
                        existedModel.UserName = model.UserName;
                        string encryptedPassword =  EncryptorDecryptor.EncryptPlainText(key,model.Password);
                        existedModel.Password = encryptedPassword;
                        existedModel.Email = model.Email;
                        existedModel.Additional = model.Additional;
                        context.UserPasswordManager.Update(existedModel);
                    }
                }
                else
                {
                    string encryptedPassword =  EncryptorDecryptor.EncryptPlainText(key,model.Password);
                    model.Password = encryptedPassword;
                    await context.UserPasswordManager.AddAsync(model);
                }
                await context.SaveChangesAsync();
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        static bool IsEmailValid(string email)
        {
            // Паттерн для валидации email
            string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

            // Создаем регулярное выражение и проверяем email
            Regex emailRegex = new Regex(pattern);
            return emailRegex.IsMatch(email);
        }

        static bool IsUrlValid(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
