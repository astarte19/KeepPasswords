using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeepPasswords.Models.TextKeeper
{
    public class NoticeItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }
        public string UserId { get; set; }

        public string Title { get; set; }
        public string? Text { get; set; } 
    }
}
