using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeepPasswords.Models.PasswordsKeeper
{
    public class PasswordItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }
        public string UserId { get; set; }
        public string WebSite { get; set; }
        public string ServiceName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public DateTime RecentDateChange { get; set; }
        public string? Additional { get; set; }
        [NotMapped]
        public string? IconURL { get; set; }

    }
}
