using HtmlAgilityPack;
using KeepPasswords.Data;
using KeepPasswords.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
            var user = await userManager.GetUserAsync(User);
            var model = context.UserPasswordManager.Where(x => x.UserId.Equals(user.Id)).ToList();
            foreach(var item in  model)
            {
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString(item.WebSite);
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlCode);

                item.IconURL = GetWebsiteIconUrl(htmlDocument, item.WebSite);
            }
            

            return PartialView("PasswordsTablePartial",model);
        }

        public async Task<IActionResult> ShowModalCreateNewPassword()
        {
            return PartialView("ModalCreatePassword");
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
    }
}
