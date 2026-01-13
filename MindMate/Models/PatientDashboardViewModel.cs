using System.Collections.Generic;

namespace MindMate.Models
{
    public class PatientDashboardViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        
        public List<Medication> Medications { get; set; } 
        
        public List<Routine> Routines { get; set; }

        public List<MemoryRecord> Memories { get; set; }
    }
}