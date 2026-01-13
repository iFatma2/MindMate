using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindMate.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Age { get; set; }

        public Guid AccessToken { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CaregiverId { get; set; }
        
        [ForeignKey("CaregiverId")]
        public ApplicationUser Caregiver { get; set; }
        
        public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
        public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();
        public virtual ICollection<MemoryRecord> MemoryRecords { get; set; } = new List<MemoryRecord>();
    }
}