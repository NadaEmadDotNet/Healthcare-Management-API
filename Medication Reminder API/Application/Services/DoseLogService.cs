using AutoMapper;
using Medication_Reminder_API.Application.DTOS;
using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Domain.Enums;
using Medication_Reminder_API.Domain.Models;

public class DoseLogService : IDoseLogService
{
    private readonly IDoseLogRepository _doseRepo;
    private readonly IMapper _mapper;

    public DoseLogService(IDoseLogRepository doseRepo, IMapper mapper)
    {
        _doseRepo = doseRepo;
        _mapper = mapper;
    }

    public PagedResult<DoseLogDTO> GetDosesForPatient(int patientId, DoseStatus? status = null, int page = 1, int pageSize = 10)
    {
        var doses = _doseRepo.GetAll()
            .Where(d => d.PatientID == patientId);

        if (status.HasValue)
            doses = doses.Where(d => d.Status == status.Value);

        var totalCount = doses.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedDoses = doses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var data = _mapper.Map<List<DoseLogDTO>>(pagedDoses);

        return new PagedResult<DoseLogDTO>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Data = data
        };
    }

    public DoseLogDTO? GetDoseById(int id)
    {
        var dose = _doseRepo.GetAll()
            .FirstOrDefault(d => d.DoseLogID == id);

        if (dose == null) return null;

        var doseDTO = _mapper.Map<DoseLogDTO>(dose);
        doseDTO.PatientName = dose.Patient.Name;
        doseDTO.MedicationName = dose.Medication.Name;
        return doseDTO;
    }

    public List<DoseLogDTO> GetDosesByMedicationName(int patientId, string name)
    {
        var doses = _doseRepo.GetAll()
            .Where(d => d.PatientID == patientId && d.Medication.Name.Contains(name));

        return _mapper.Map<List<DoseLogDTO>>(doses.ToList());
    }

    public DoseLogDTO AddDose(AddDoseDTO dto)
    {
        // نفس اللوجيك
        var med = _doseRepo.GetAll().FirstOrDefault(m => m.MedicationID == dto.MedicationID)?.Medication;
        if (med == null)
            throw new ArgumentException("Medication not found.");

        var patient = _doseRepo.GetAll().FirstOrDefault(p => p.PatientID == dto.PatientID)?.Patient;
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var patientMed = _doseRepo.GetAll()
            .FirstOrDefault(pm => pm.PatientID == dto.PatientID && pm.MedicationID == dto.MedicationID);
        if (patientMed == null)
            throw new ArgumentException("Medication is not assigned to this patient.");

        if (dto.ScheduledTime < DateTime.Now)
            throw new ArgumentException("Scheduled time must be in the future.");

        var dose = new DoseLog
        {
            PatientID = dto.PatientID,
            MedicationID = dto.MedicationID,
            ScheduledTime = dto.ScheduledTime,
            Status = DoseStatus.Scheduled,
            Notes = dto.Notes
        };

        _doseRepo.AddAsync(dose).Wait();
        _doseRepo.SaveChangesAsync().Wait();

        var doseDTO = _mapper.Map<DoseLogDTO>(dose);
        doseDTO.PatientName = patient.Name;
        doseDTO.MedicationName = med.Name;

        return doseDTO;
    }

    public DoseLogDTO? UpdateTakenTime(int doseLogId, DateTime takenTime, IMedicationService medService)
    {
        var dose = _doseRepo.GetAll()
            .FirstOrDefault(d => d.DoseLogID == doseLogId);

        if (dose == null) return null;

        dose.TakenTime = takenTime;

        // تحديث حالة الجرعة بناءً على الوقت
        var endOfDay = dose.ScheduledTime.Date.AddDays(1);
        dose.Status = takenTime <= endOfDay ? DoseStatus.Taken : DoseStatus.Missed;

        _doseRepo.SaveChangesAsync().Wait();

        medService.UpdateStatusAsync(dose.MedicationID);

        var dto = _mapper.Map<DoseLogDTO>(dose);
        dto.PatientName = dose.Patient.Name;
        dto.MedicationName = dose.Medication.Name;

        return dto;
    }

    public int GetTakenDoseCount(int medicationId)
    {
        return _doseRepo.GetAll().Count(d => d.MedicationID == medicationId && d.Status == DoseStatus.Taken);
    }
}
