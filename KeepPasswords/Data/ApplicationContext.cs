using System.Collections.Generic;
using System.Threading.Tasks;
using KeepPasswords.Models.Account;
using KeepPasswords.Models.PasswordsKeeper;
using KeepPasswords.Models.TextKeeper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace KeepPasswords.Data
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<UserSecretPhrase> UserSecretPhrases { get; set; }
        public DbSet<UserAvatar> UserAvatars { get; set; }
        public DbSet<PasswordItem> UserPasswordManager { get; set; }
        public DbSet<NoticeItem> UserNotices { get; set; }
    }
}
