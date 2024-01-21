using KeepPasswords.Data;
using KeepPasswords.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace KeepPasswords.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationContext context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.context = context;
        }
        [HttpGet]

        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Username };
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    await _signInManager.SignInAsync(user, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if(User.Identity.IsAuthenticated)
            {
                if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {

                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {

                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Wrong data");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> AccountSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            var avatar = context.UserAvatars.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().Avatar;
            ViewBag.Avatar = avatar;
            return View("AccountSettings", user);
        }
        [Authorize]
        public async Task<IActionResult> SaveAccountChanges([Bind] ProfileData profile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);                
                user.UserName = profile.UserName;
                user.Email = profile.Email;
                await _userManager.UpdateAsync(user);

                

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return Content($"{ex.Message}");
            }
        }
        [Authorize]
        public async Task<IActionResult> UploadAvatar()
        {
            try
            {
                var Avatar = Request.Form.Files[0];
                if (Avatar != null)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var currentAvatar = context.UserAvatars.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();
                    if (currentAvatar != null)
                    {
                        context.UserAvatars.Remove(currentAvatar);
                        await context.SaveChangesAsync();
                    }

                    byte[] imageData = null;

                    using (var binaryReader = new BinaryReader(Avatar.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)Avatar.Length);
                    }
                   
                    UserAvatar userAvatar = new UserAvatar();
                    userAvatar.UserId = user.Id;
                    userAvatar.Avatar = imageData;
                    context.UserAvatars.Add(userAvatar);
                    await context.SaveChangesAsync();
                    return new EmptyResult();
                }
                return Content("Аватар не был загружен!");
            }
            catch(Exception ex)
            {
                return Content(ex.Message);
            }
            
        }

        [Authorize]
        public async Task<IActionResult> IsEmailFree(string email)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser.Email.Equals(email))
            {
                return new EmptyResult();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return Content("Email занят");
            }

            return new EmptyResult();
        }
    }
}
