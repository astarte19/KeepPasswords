using KeepPasswords.Data;
using KeepPasswords.Models;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.Calendar;
using KeepPasswords.Models.TextKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
            var lst = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.Date >= DateStart.AddDays(-5) && x.Date <= DateEnd).ToList();               
            var serialize = JsonConvert.SerializeObject(lst, new JsonSerializerSettings() { MaxDepth = Int32.MaxValue, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return new JsonResult(serialize);
        }

        public async Task<IActionResult> ShowModalDayEvents(string currentDate)
        {
            var user = await userManager.GetUserAsync(User);
            DateTime date = Convert.ToDateTime(currentDate);
            List<CalendarItem> model = context.UserCalendarEvents.Where(x => x.UserId.Equals(user.Id) && x.Date.Date == date.Date).ToList();
            ViewBag.Date = date.ToString("dd.MM.yyyy");
            return PartialView("ModalDateEvents", model);
        }
    }
}
