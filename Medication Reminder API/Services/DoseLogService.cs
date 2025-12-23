using AutoMapper;
using Medication_Reminder_API.DTOS;
using Medication_Reminder_API.Enums;
using Medication_Reminder_API.Models;
using Medication_Reminder_API.Services;
using Microsoft.EntityFrameworkCore;

public class DoseLogService : IDoseLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DoseLogService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<DoseLogDTO> GetDosesForPatient(int patientId, DoseStatus? status = null)
    {
        var doses = _context.DoseLogs
            .Include(d => d.Medication)
            .Where(d => d.PatientID == patientId);

        if (status.HasValue)
            doses = doses.Where(d => d.Status == status.Value);

        return _mapper.Map<List<DoseLogDTO>>(doses.ToList());
    }
    public DoseLogDTO? GetDoseById(int id)
    {
        var dose = _context.DoseLogs
            .Include(d => d.Patient)
            .Include(d => d.Medication)
            .FirstOrDefault(d => d.DoseLogID == id);

        if (dose == null) return null;

        var doseDTO = _mapper.Map<DoseLogDTO>(dose);

        doseDTO.PatientName = dose.Patient.Name;
        doseDTO.MedicationName = dose.Medication.Name;

        return doseDTO;
    }

public List<DoseLogDTO> GetDosesByMedicationName(int patientId, string name)
    {
        var doses = _context.DoseLogs
            .Include(d => d.Medication)
            .Where(d => d.PatientID == patientId && d.Medication.Name.Contains(name));

        return _mapper.Map<List<DoseLogDTO>>(doses.ToList());
    }
    public DoseLogDTO AddDose(AddDoseDTO dto)
    {
        var med = _context.Medications.Find(dto.MedicationID);
        if (med == null)
            throw new ArgumentException("Medication not found.");

        var patient = _context.Patients.Find(dto.PatientID);
        if (patient == null)
            throw new ArgumentException("Patient not found.");

        var patientMed = _context.PatientMedications
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

        _context.DoseLogs.Add(dose);
        _context.SaveChanges();

        var doseDTO = _mapper.Map<DoseLogDTO>(dose);


        doseDTO.PatientName = patient.Name;
        doseDTO.MedicationName = med.Name;

        return doseDTO;
    }

    public DoseLogDTO? UpdateTakenTime(int doseLogId, DateTime takenTime, IMedicationService medService)
    {
        var dose = _context.DoseLogs
            .Include(d => d.Medication)
            .Include(d => d.Patient)
            .FirstOrDefault(d => d.DoseLogID == doseLogId);

        if (dose == null) return null;

        // تحديث TakenTime
        dose.TakenTime = takenTime;

        // تحديث حالة الجرعة بناءً على الوقت
        var endOfDay = dose.ScheduledTime.Date.AddDays(1); // نهاية اليوم نفسه
        if (takenTime <= endOfDay)
            dose.Status = DoseStatus.Taken;
        else
            dose.Status = DoseStatus.Missed;

        _context.SaveChanges();

        // تحديث حالة الدواء بعد أي تعديل على الجرعة
        medService.UpdateStatus(dose.MedicationID);

        var dto = _mapper.Map<DoseLogDTO>(dose);
        dto.PatientName = dose.Patient.Name;
        dto.MedicationName = dose.Medication.Name;

        return dto;
    }

    public int GetTakenDoseCount(int medicationId)
    {
        return _context.DoseLogs.Count(d => d.MedicationID == medicationId && d.Status == DoseStatus.Taken);
    }
}
