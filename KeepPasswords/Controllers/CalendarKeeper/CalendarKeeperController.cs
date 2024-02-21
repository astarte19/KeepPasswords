using KeepPasswords.Data;
using KeepPasswords.Models;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.Calendar;
using KeepPasswords.Models.TextKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace KeepPasswords.Controllers.CalendarKeeper
{
    [Authorize]
    public class CalendarKeeperController : Controller
    {
        private readonly ApplicationContext context;
        private readonly UserManager<User> userManager;
        public CalendarKeeperController(ApplicationContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<JsonResult> GetCalendarData(string CurrentYear, string CurrentMounth)
        {

            var DateStart = DateTime.Parse($"{CurrentYear}.{CurrentMounth}.01");
            var DateEnd = DateStart.AddMonths(1).AddDays(5);
            var user = await userManager.GetUserAsync(User);
            var secretPhrase = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();
            var lst = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.Date >= DateStart.AddDays(-5) && x.Date <= DateEnd).ToList();

            if (secretPhrase != null)
            {
                string key = secretPhrase.SecretPhrase;

                foreach (var item in lst)
                {
                    item.EventNameDecrypted = EncryptorDecryptor.DecryptToPlainText(key, item.EventName);
                    item.DescriptionDecrypted = item.Description == null ? "" : EncryptorDecryptor.DecryptToPlainText(key, item.Description);
                }
            }

            
            var serialize = JsonConvert.SerializeObject(lst, new JsonSerializerSettings() { MaxDepth = Int32.MaxValue, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return new JsonResult(serialize);
        }

        public async Task<IActionResult> ShowModalDayEvents(string currentDate)
        {
            var user = await userManager.GetUserAsync(User);
            DateTime date = DateTime.ParseExact(currentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            List<CalendarItem> model = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.Date.Date == date.Date).ToList();
            
            var secretPhrase = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();
            if(secretPhrase != null)
            {
                string key = secretPhrase.SecretPhrase;
                foreach (var item in model)
                {
                    item.EventNameDecrypted = EncryptorDecryptor.DecryptToPlainText(key, item.EventName);
                    item.DescriptionDecrypted = item.Description == null ? "" : EncryptorDecryptor.DecryptToPlainText(key, item.Description);
                }
            }
            else
            {
                model = new List<CalendarItem>();
            }
            
            
            ViewBag.Date = date.ToString("dd.MM.yyyy");
            return PartialView("ModalDateEvents", model);
        }

        public async Task<IActionResult> ShowModalCreateEvent(string currentDate)
        {
            try
            {
                CalendarItem item = new CalendarItem();
                DateTime date = DateTime.ParseExact(currentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                item.Date = new DateTime(date.Year, date.Month, date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                item.Color = "#3674ab";
                item.EventName = "";
                item.ItemId = 0;
                return PartialView("ModalAddEditEvent", item);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> ShowModalUpdateEvent(int ItemId)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var model = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.ItemId == ItemId).FirstOrDefault();

                if (model == null)
                {
                    return new EmptyResult();
                }

                string key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().SecretPhrase;
                model.DescriptionDecrypted = model.Description == null ? "" : EncryptorDecryptor.DecryptToPlainText(key, model.Description);
                model.EventNameDecrypted = EncryptorDecryptor.DecryptToPlainText(key, model.EventName);

                return PartialView("ModalAddEditEvent", model);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> AddUpdateEvent([Bind] CalendarItem model)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                string key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().SecretPhrase;

                if(String.IsNullOrEmpty(key))
                {
                    return Content("Для пользователя не установлен SecretPhrase! Необходимо добавить его в настройках!");
                }

                if (model.ItemId == 0)
                {
                    if(String.IsNullOrEmpty(model.EventName))
                    {
                        return Content("Название события обязательно к заполнению!");
                    }

                    if(String.IsNullOrEmpty(model.Color))
                    {
                        return Content("Цвет события обязателен к выбору!");
                    }

                    model.UserId = user.Id;
                    model.EventName = EncryptorDecryptor.EncryptPlainText(key, model.EventName);
                    model.Description = EncryptorDecryptor.EncryptPlainText(key, model.Description);
                    
                    await context.UserCalendarEvents.AddAsync(model);               
                }
                else
                {
                    var existedEvent = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.ItemId == model.ItemId).FirstOrDefault();
                    
                    if(existedEvent == null) 
                    {
                        return Content("Событие не найдено или не принадлежит пользователю!");
                    }

                    if (String.IsNullOrEmpty(model.EventName))
                    {
                        return Content("Название события обязательно к заполнению!");
                    }

                    if (String.IsNullOrEmpty(model.Color))
                    {
                        return Content("Цвет события обязателен к выбору!");
                    }

                    existedEvent.EventName = EncryptorDecryptor.EncryptPlainText(key, model.EventName); ;
                    existedEvent.Color = model.Color;
                    existedEvent.Date = model.Date;
                    existedEvent.Description = EncryptorDecryptor.EncryptPlainText(key, model.Description == null ? "" : model.Description);
                    context.UserCalendarEvents.Update(existedEvent);
                    
                    
                }
                await context.SaveChangesAsync();
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public async Task<IActionResult> DeleteEvent(int ItemId)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var model = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.ItemId == ItemId).FirstOrDefault();
                if (model == null)
                {
                    return Content("Событие не найдено ил не принадлежит пользователю!");
                }

                context.UserCalendarEvents.Remove(model);
                await context.SaveChangesAsync();
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }

    public class CalendarPartial : ViewComponent
    {
        private readonly ApplicationContext applicationContext;
        private readonly UserManager<User> _userManager;

        public CalendarPartial(ApplicationContext applicationContext, UserManager<User> _userManager)
        {
            this.applicationContext = applicationContext;
            this._userManager = _userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            bool hasSecretPhrase = applicationContext.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).Count() > 0 ? true : false;
            ViewBag.HasSecretPhrase = hasSecretPhrase;
            return View("~/Views/CalendarKeeper/Components/CalendarPartial/CalendarPartial.cshtml");
        }
    }
}
