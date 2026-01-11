using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Services;
using Microsoft.AspNetCore.Authorization;
namespace Medication_Reminder_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : BaseController
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService, ApplicationDbContext context) : base(context)
        {
            _patientService = patientService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor, Patient,Caregiver")]
        public async Task<IActionResult> GetAllPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            var patients = await _patientService.GetAllPatientsAsync(
                doctorId: role == "Doctor" ? userId : null,
                caregiverId: role == "Caregiver" ? userId : null,
                patientId: role == "Patient" ? userId : null,
                page: page,
                pageSize: pageSize
            );

            if (patients.Data == null || !patients.Data.Any())
                return NotFound("No patients found.");

            return Ok(patients);
        }


        [HttpGet("ByName")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            var patients = await _patientService.GetByNameAsync(name);

            if (!patients.Any())
                return NotFound($"No patients found with name containing '{name}'.");

            return Ok(patients);
        }


        [HttpPost("{patientId}/AssignMedication/{medicationId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AssignMedicationToPatient(int patientId, int medicationId)
        {
            // Assign medication once
            var result = await _patientService.AssignMedicationToPatientAsync(patientId, medicationId);

            if (!result.Success)
                return BadRequest(result.Message);

            // Generate doses for the new assignment
            await _patientService.GenerateDosesForNewAssignmentAsync(patientId, medicationId);

            return Ok(result.Message);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> EditPatient(int id, [FromBody] PatientDto patientDto)
        {
            var updatedPatient = await _patientService.EditPatientAsync(id, patientDto);
            if (updatedPatient == null)
                return NotFound("Patient not found.");

            return Ok(updatedPatient);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var deletedPatient = await _patientService.DeletePatientAsync(id);
            if (deletedPatient == null)
                return NotFound("Patient not found.");

            return Ok(deletedPatient);
        }

    }
}
