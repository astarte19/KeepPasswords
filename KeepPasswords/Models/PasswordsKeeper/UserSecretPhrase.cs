using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeepPasswords.Models.PasswordsKeeper
{
    public class UserSecretPhrase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }
            
        public string UserId { get; set; }
        public string SecretPhrase { get; set; }
    }
}
