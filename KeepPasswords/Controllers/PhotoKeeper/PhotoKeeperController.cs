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
using Microsoft.AspNetCore.Authorization;
using KeepPasswords.Models.PhotoKeeper;
using KeepPasswords.Models;
using System.Text.Unicode;

namespace KeepPasswords.Controllers.PhotoKeeper
{
    [Authorize]
    public class PhotoKeeperController : Controller
    {
        private readonly ApplicationContext context;
        private readonly UserManager<User> userManager;
        public PhotoKeeperController(ApplicationContext context, UserManager<User> userManager)
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

        public async Task<IActionResult> GetPhotos()
        {
            var user = await userManager.GetUserAsync(User);
            var secretPhrase = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault();
            var model = context.UserPhotos.Where(x => x.UserId.Equals(user.Id)).ToList();
            if (secretPhrase != null)
            {
                string key = secretPhrase.SecretPhrase;
                foreach (var item in model)
                {
                    UTF8Encoding utf8 = new UTF8Encoding();
                    item.DecryptedPhotoBytes = EncryptorDecryptor.DecryptBytes(item.PhotoBytes, utf8.GetBytes(key));
                }
            }
                                   
            return PartialView("PhotoGridPartial", model);
        }
        public async Task<IActionResult> ShowModalSuccess(string message)
        {
            return PartialView("ModalSuccess", message);
        }

        public async Task<IActionResult> ShowModalUploadPhoto()
        {
            return PartialView("ModalUploadPhoto");
        }

        public async Task<IActionResult> DeletePhoto()
        {
            try
            {
                int ItemId = 0;
                if (HttpContext.Request.Cookies["SelectedPhotoItemId"] != null)
                {
                    ItemId = Convert.ToInt32(HttpContext.Request.Cookies["SelectedPhotoItemId"]);
                }
                if(ItemId == 0)
                {
                    return Content("Возникла ошибка при удалении фото, фото не было удалено! Повторите попытку позже!");
                }
                var user = await userManager.GetUserAsync(User);
                var photo = context.UserPhotos.Where(x => x.ItemId == ItemId && x.UserId.Equals(user.Id)).FirstOrDefault();

                if(photo == null)
                {
                    return Content("Возникла ошибка при удалении фото! Фото не найдено или не принадлежит пользователю!");
                }

                context.UserPhotos.Remove(photo);
                await context.SaveChangesAsync();
                return new EmptyResult();    
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        public async Task<IActionResult> UploadPhoto()
        {
            try
            {
                var Photo = Request.Form.Files[0];                
                if (Photo != null)
                {
                    var user = await userManager.GetUserAsync(User);
                    string key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().SecretPhrase;
                    byte[] imageData = null;

                    using (var binaryReader = new BinaryReader(Photo.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)Photo.Length);
                    }
                    UTF8Encoding utf8 = new UTF8Encoding();
                    PhotoItem photoItem = new PhotoItem();
                    photoItem.UserId = user.Id;
                    photoItem.PhotoBytes = EncryptorDecryptor.EncryptBytes(imageData, utf8.GetBytes(key));
                    photoItem.FileName = Photo.FileName;
                    context.UserPhotos.Add(photoItem);
                    await context.SaveChangesAsync();
                    return new EmptyResult();
                }
                return Content("Файл поврежден! Фото не было загружено.");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

        }
    }
}
