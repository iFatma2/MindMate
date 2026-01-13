namespace MindMate.Models;

public class Routine
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Now;
    public bool IsCompleted { get; set; } = false;

    public virtual Patient? Patient { get; set; }
}