namespace Medication_Reminder_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected string GetCurrentUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        protected string GetCurrentUserRole() =>
            User.FindFirst(ClaimTypes.Role)?.Value;

        // ✨ التعديل هنا
        protected async Task<bool> CanAccessPatientAsync(int patientId)
        {
            var role = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            if (role == "Admin")
                return true;

            if (role == "Patient")
                return await _context.Patients
                    .AnyAsync(p => p.PatientID == patientId && p.UserId == userId);

            if (role == "Caregiver")
                return await _context.PatientCaregivers
                    .AnyAsync(pc => pc.PatientID == patientId && pc.Caregiver.UserId == userId);

            if (role == "Doctor")
                return await _context.DoctorPatients
                    .AnyAsync(dp => dp.PatientId == patientId && dp.Doctor.UserId == userId);

            return false;
        }
    }
}
