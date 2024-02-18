using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KeepPasswords.Models.Calendar
{
    public class CalendarItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }
        public string UserId { get; set; }
        public string EventName { get; set; }
        public DateTime Date { get; set; }
        public string Color { get; set; }
        public string? Description { get; set; }

    }
}
