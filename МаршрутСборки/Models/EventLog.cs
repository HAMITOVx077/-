using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class EventLog
    {
        [Key]
        public int LogId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}