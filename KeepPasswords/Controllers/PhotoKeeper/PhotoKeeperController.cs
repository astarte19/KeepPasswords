﻿using HtmlAgilityPack;
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
            string key = context.UserSecretPhrases.Where(x => x.UserId.Equals(user.Id)).FirstOrDefault().SecretPhrase;
            var model = context.UserPhotos.Where(x => x.UserId.Equals(user.Id)).ToList();
            foreach(var item in model)
            {
                item.DecryptedPhotoBytes = item.PhotoBytes;//EncryptorDecryptor.DecryptBytes(Convert.FromBase64String(key), item.PhotoBytes);
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

                    PhotoItem photoItem = new PhotoItem();
                    photoItem.UserId = user.Id;
                    photoItem.PhotoBytes = imageData;//EncryptorDecryptor.EncryptBytes(Convert.FromBase64String(key), imageData);
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
