using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class WarrantyCaseNote
    {
        [Key]
        public int NoteId { get; set; }
        public int CaseId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int AuthorId { get; set; }

        public WarrantyCase Case { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}
