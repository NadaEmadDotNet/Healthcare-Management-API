using Medication_Reminder_API.Domain.Enums;

public class DoseLogDTO
{
    public int DoseLogID { get; set; }          
    public int PatientID { get; set; }
    public string PatientName { get; set; }
    public int MedicationID { get; set; }
    public string MedicationName { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? TakenTime { get; set; }
    public DoseStatus Status { get; set; }
    public string? Notes { get; set; }
}
