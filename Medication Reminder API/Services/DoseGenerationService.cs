using Medication_Reminder_API.Models;
using Medication_Reminder_API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Medication_Reminder_API.Services
{
    public class TestDoseGenerationService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TestDoseGenerationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task ExecuteOnceAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var activeMedications = await db.Medications
                .Where(m => m.Status == MedicationStatus.Ongoing)
                .ToListAsync();

            var today = DateTime.Today;

            foreach (var med in activeMedications)
            {
                var patientMeds = await db.PatientMedications
                    .Include(pm => pm.Patient)
                    .Where(pm => pm.MedicationID == med.MedicationID)
                    .ToListAsync();

                foreach (var pm in patientMeds)
                {
                    var patient = pm.Patient;

                    bool alreadyGenerated = await db.DoseLogs
                        .AnyAsync(d => d.MedicationID == med.MedicationID
                                       && d.PatientID == patient.PatientID
                                       && d.ScheduledTime.Date == today);

                    if (!alreadyGenerated)
                    {
                        double interval = 24.0 / med.Frequency;

                        for (int i = 0; i < med.Frequency; i++)
                        {
                            db.DoseLogs.Add(new DoseLog
                            {
                                MedicationID = med.MedicationID,
                                PatientID = patient.PatientID,
                                Status = DoseStatus.Scheduled,
                                ScheduledTime = today.AddHours(i * interval)
                            });
                        }
                    }
                }
            }

            await db.SaveChangesAsync();
            Console.WriteLine("Daily doses generated successfully!");
        }
    }
}
