using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;


namespace Medication_Reminder_API.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class CareGiverController : ControllerBase
    {
        private readonly ICaregiverService _caregiverService;
        private readonly IMapper _mapper;

        public CareGiverController(ICaregiverService caregiverService, IMapper mapper)
        {
            _caregiverService = caregiverService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var caregivers = await _caregiverService.GetAllCaregiversAsync();
            if (!caregivers.Any())
                return NotFound("No caregivers found.");

            return Ok(caregivers);
        }

        [HttpGet("MyPatients")]
        [Authorize(Roles = "Caregiver,Admin")]
        public async Task<IActionResult> GetMyPatients()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var patients = await _caregiverService.GetPatientsWithMedicationsAsync(userId);
            if (!patients.Any())
                return NotFound("No patients assigned.");

            return Ok(patients);
        }

        [HttpGet("search")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            var caregiver = await _caregiverService.GetCaregiverByNameAsync(name);
            if (caregiver == null)
                return NotFound($"No caregiver found with name '{name}'.");

            return Ok(caregiver);
        }

        [HttpPost("AssignPatient")]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> AssignPatient([FromBody] CaregiverAssignDTO dto)
        {
            try
            {
                var result = await _caregiverService.AssignPatientToCaregiverAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(int id, [FromBody] Caregiver caregiver)
        {
            var updated = await _caregiverService.EditCaregiverAsync(id, caregiver);
            if (updated == null)
                return NotFound("Caregiver not found.");

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _caregiverService.DeleteCaregiverAsync(id);
            if (deleted == null)
                return NotFound("Caregiver not found.");

            return NoContent();
        }

    }
}
