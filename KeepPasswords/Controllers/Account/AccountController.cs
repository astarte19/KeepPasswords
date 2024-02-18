using KeepPasswords.Data;
using KeepPasswords.Models.PasswordsKeeper;
using KeepPasswords.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using KeepPasswords.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;

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
            var avatar = context.UserAvatars.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();
            if (avatar != null)
            {
                ViewBag.Avatar = avatar.Avatar;
            }
            else
            {
                ViewBag.Avatar = null;
            }
            var secretKey = context.UserSecretPhrases.Where(x=>x.UserId.Equals(user.Id)).FirstOrDefault();
            if(secretKey != null)
            {
                ViewBag.SecretKey = secretKey.SecretPhrase;
            }
            else
            {
                ViewBag.SecretKey = null;
            }

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
                user.Location = profile.Location;
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
        [Authorize]
        public async Task<IActionResult> ShowModalResetAvatarConfirmation()
        {
            return PartialView("ModalResetAvatarConfirmation");
        }
        [Authorize]
        public async Task<IActionResult> ResetAvatar()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var avatar = context.UserAvatars.Where(x=>x.UserId.Equals(currentUser.Id)).FirstOrDefault();
                if (avatar != null)
                {
                    context.UserAvatars.Remove(avatar);
                    await context.SaveChangesAsync();
                }
                return new EmptyResult();
            }
            catch(Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [Authorize]
        public async Task<IActionResult> ShowModalResetPassword()
        {
            return PartialView("ModalResetPassword");
        }

        [Authorize]
        public async Task<IActionResult> ChangeUserPassword(string Password)
        {
            try
            {                
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, code, Password);
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [Authorize]
        public async Task<IActionResult> ShowModalSecretKey()
        {
            return PartialView("ModalSecretKey");
        }

        [Authorize] 
        public async Task<IActionResult> AddUpdateSecretPhrase(string SecretPhrase)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var secretKey = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();

                if(String.IsNullOrEmpty(SecretPhrase) || SecretPhrase.Length != 16)
                {
                    return Content("Secret Phrase должен иметь длину 16 символов!");
                }

                if(secretKey != null)
                {
                    await ChangePasswordManagerUserDataCipher(secretKey.SecretPhrase,SecretPhrase);
                    await ChangeTextManagerUserDataCipher(secretKey.SecretPhrase, SecretPhrase);
                    await ChangePhotoManagerUserDataCipher(secretKey.SecretPhrase, SecretPhrase);
                    secretKey.SecretPhrase = SecretPhrase;
                    context.UserSecretPhrases.Update(secretKey);                    
                }
                else
                {
                    UserSecretPhrase userSecretPhrase = new UserSecretPhrase();
                    userSecretPhrase.UserId = user.Id;
                    userSecretPhrase.SecretPhrase = SecretPhrase;
                    await context.UserSecretPhrases.AddAsync(userSecretPhrase);
                }
                await context.SaveChangesAsync();
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [Authorize]
        public async Task ChangePasswordManagerUserDataCipher(string oldSecretKey,string newSecretKey)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userPasswordManager = context.UserPasswordManager.Where(x => x.UserId.Equals(user.Id));
            foreach(var item in userPasswordManager)
            {
                string decryptedPassword = EncryptorDecryptor.DecryptToPlainText(oldSecretKey, item.Password);
                string encryptedPassword = EncryptorDecryptor.EncryptPlainText(newSecretKey, decryptedPassword);
                item.Password = encryptedPassword;
            }
            context.UserPasswordManager.UpdateRange(userPasswordManager);
            await context.SaveChangesAsync();

        }

        [Authorize]
        public async Task ChangeTextManagerUserDataCipher(string oldSecretKey, string newSecretKey)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userNotices = context.UserNotices.Where(x => x.UserId.Equals(user.Id));
            foreach (var item in userNotices)
            {
                string decryptedTitle = EncryptorDecryptor.DecryptToPlainText(oldSecretKey, item.Title);
                string encryptedTitle = EncryptorDecryptor.EncryptPlainText(newSecretKey, decryptedTitle);
                item.Title = encryptedTitle;

                string decryptedText = EncryptorDecryptor.DecryptToPlainText(oldSecretKey, item.Text);
                string encryptedText = EncryptorDecryptor.EncryptPlainText(newSecretKey, decryptedText);
                item.Text = encryptedText;
            }
            context.UserNotices.UpdateRange(userNotices);
            await context.SaveChangesAsync();

        }

        [Authorize]
        public async Task ChangePhotoManagerUserDataCipher(string oldSecretKey, string newSecretKey)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userPhotos = context.UserPhotos.Where(x => x.UserId.Equals(user.Id));
            foreach (var item in userPhotos)
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] decryptedBytes = EncryptorDecryptor.DecryptBytes(item.PhotoBytes, utf8.GetBytes(oldSecretKey));
                byte[] encryptedBytes = EncryptorDecryptor.EncryptBytes(decryptedBytes, utf8.GetBytes(newSecretKey));
                item.PhotoBytes = encryptedBytes;
            }
            context.UserPhotos.UpdateRange(userPhotos);
            await context.SaveChangesAsync();

        }
    }
}
