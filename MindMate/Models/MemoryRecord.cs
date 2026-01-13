using System.ComponentModel.DataAnnotations;

namespace MindMate.Models
{
    public class MemoryRecord
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        public string? ImagePath { get; set; }
        
        public string? AudioPath { get; set; }

        [Required]
        public string Caption { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Patient? Patient { get; set; }
    }
}