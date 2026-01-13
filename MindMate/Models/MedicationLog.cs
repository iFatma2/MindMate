using System.ComponentModel.DataAnnotations;

namespace MindMate.Models
{
    public class MedicationLog
    {
        public int Id { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TakenDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime TakenTime { get; set; }

        [Required]
        public string Status { get; set; }

        // Navigation Property
        public virtual Medication Medication { get; set; }
    }
}
