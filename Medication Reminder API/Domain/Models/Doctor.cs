using Medication_Reminder_API.Infrastructure;

public class Doctor
{
    public int DoctorId { get; set; }
    public string Name { get; set; }
    public string Specialty { get; set; }
    public string UserId { get; set; }  
    public ApplicationUser User { get; set; }
    public ICollection<DoctorPatient> DoctorPatients { get; set; }

}
