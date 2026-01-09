using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Medication_Reminder_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IMedicationService _medService;

        public MedicationsController(ApplicationDbContext context, IMapper mapper, IMedicationService medService)
            : base(context)
        {
            _mapper = mapper;
            _medService = medService;
        }

        [HttpGet]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetAllMedications([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var meds = await _medService.GetAllMedicationsAsync(page, pageSize);
            return Ok(meds);
        }


        [HttpGet("search")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var meds = await _medService.GetByNameAsync(name);
            if (!meds.Any())
                return NotFound($"No medications found containing '{name}'.");

            return Ok(meds);
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Patient,Doctor,Caregiver,Admin")]
        public async Task<IActionResult> GetAllMedicationsForPatient(int patientId)
        {
            if (!await CanAccessPatientAsync(patientId)) return Forbid();

            var meds = await _medService.GetAllMedicationsForPatientAsync(patientId);
            if (!meds.Any())
                return NotFound("No medications found for this patient.");

            return Ok(meds);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Add([FromBody] MedicationDTO dto)
        {
            var med = await _medService.AddAsync(dto);
            return CreatedAtAction(nameof(GetByName), new { name = med.Name }, med);
        }

        [HttpPut("{medicationId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> EditMedication(int medicationId, [FromBody] MedicationDTO medication)
        {
            var updatedMed = await _medService.EditMedicationAsync(medicationId, medication);
            if (updatedMed == null)
                return NotFound("Medication not found.");

            return Ok(updatedMed);
        }

        [HttpDelete("{medicationId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteMedication(int medicationId)
        {
            var deletedMed = await _medService.DeleteMedicationAsync(medicationId);
            if (deletedMed == null)
                return NotFound("Medication not found.");

            return NoContent();
        }
    }
}
