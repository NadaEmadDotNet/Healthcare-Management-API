using Medication_Reminder_API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Medication_Reminder_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoseLogController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IDoseLogService _doseLogService;

        public DoseLogController(ApplicationDbContext context, IMapper mapper, IDoseLogService doseLogService)
            : base(context)
        {
            _mapper = mapper;
            _doseLogService = doseLogService;
        }

        [HttpGet("Patient/{patientId}")]
        public async Task<IActionResult> GetDoses(int patientId, int page = 1, int pageSize = 10)
        {
            if (!await CanAccessPatientAsync(patientId))
                return Forbid();

            var doses = _doseLogService.GetDosesForPatient(patientId, null, page, pageSize);

            if (!doses.Data.Any())
                return NotFound("No doses found for this patient.");

            return Ok(doses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoseById(int id)
        {
            var dose = _doseLogService.GetDoseById(id);
            if (dose == null) return NotFound();

            if (!await CanAccessPatientAsync(dose.PatientID)) return Forbid();
            return Ok(dose);
        }

        [HttpGet("Patient/{patientId}/Status")]
        public async Task<IActionResult> GetDosesByStatus(int patientId, [FromQuery] DoseStatus status, int page, int pagesize)
        {
            if (!await CanAccessPatientAsync(patientId)) return Forbid();

            var doses = _doseLogService.GetDosesForPatient(patientId, status, page, pagesize);
            if (doses.Data == null || !doses.Data.Any())
                return NotFound($"No {status} doses found for this patient.");
            return Ok(doses);
        }

        [HttpGet("Patient/{patientId}/Search")]
        public async Task<IActionResult> GetDosesByMedicationName(int patientId, [FromQuery] string name)
        {
            if (!await CanAccessPatientAsync(patientId)) return Forbid();

            var doses = _doseLogService.GetDosesByMedicationName(patientId, name);
            if (!doses.Any())
                return NotFound($"No doses found with medication name containing '{name}'.");
            return Ok(doses);
        }

        [HttpPost("AddDose")]
        public async Task<ActionResult<DoseLogDTO>> AddDose([FromBody] AddDoseDTO dto)
        {
            if (!await CanAccessPatientAsync(dto.PatientID)) return Forbid();

            try
            {
                var dose = _doseLogService.AddDose(dto);
                return Ok(dose);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/TakenTime")]
        public async Task<IActionResult> UpdateTakenTime(int id, [FromBody] DateTime takenTime, [FromServices] IMedicationService medService, int page, int pagesize)
        {
            var updatedDose = _doseLogService.UpdateTakenTime(id, takenTime, medService);
            if (updatedDose == null) return NotFound("Dose not found.");

            if (!await CanAccessPatientAsync(updatedDose.PatientID)) return Forbid();

            var allDoses = _doseLogService.GetDosesForPatient(updatedDose.PatientID, null, page, pagesize);
            return Ok(allDoses);
        }

        [HttpGet("medications/{medicationId}/doses/takenCount")]
        public IActionResult GetTakenDoseCount(int medicationId)
        {
            var count = _doseLogService.GetTakenDoseCount(medicationId);
            return Ok(count);
        }
    }
}
