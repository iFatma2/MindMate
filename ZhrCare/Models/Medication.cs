
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZhrCare.Models
{
    public class Medication
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Dosage { get; set; } 

        public string? FrequencyType { get; set; } 

        public string? SelectedDays { get; set; } 

        [Required]
        [DataType(DataType.Time)]
        public DateTime ScheduledTime { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today; 

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

        // Navigation Properties
        public virtual Patient? Patient { get; set; } 
        public virtual ICollection<MedicationLog> MedicationLogs { get; set; } = new List<MedicationLog>();
    }
}