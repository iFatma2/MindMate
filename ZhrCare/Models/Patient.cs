using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZhrCare.Models
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

        // الربط مع المستخدم (Caregiver)
        public string CaregiverId { get; set; }
        
        [ForeignKey("CaregiverId")]
        public ApplicationUser Caregiver { get; set; }
    }
}