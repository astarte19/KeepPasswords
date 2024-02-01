using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeepPasswords.Models.PhotoKeeper
{
    public class PhotoItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        public string UserId { get; set; }
        public byte[] PhotoBytes { get; set; }
        [NotMapped]
        public byte[] DecryptedPhotoBytes { get; set; }
        public string FileName { get; set; }
    }
}
