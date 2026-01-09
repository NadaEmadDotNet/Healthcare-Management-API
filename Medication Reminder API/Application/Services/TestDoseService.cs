//using Medication_Reminder_API.Domain.Enums;
//using Medication_Reminder_API.Domain.Models;
//using Medication_Reminder_API.Infrastructure;



//namespace Medication_Reminder_API.Services
//{
//    public class TestDoseService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;

//        public TestDoseService(IServiceScopeFactory scopeFactory)
//        {
//            _scopeFactory = scopeFactory;
//        }

//        public async Task ExecuteOnceAsync()
//        {
//            using var scope = _scopeFactory.CreateScope();
//            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//            var activeMedications = await db.Medications
//                .Where(m => m.Status == MedicationStatus.Ongoing)
//                .ToListAsync();

//            var today = DateTime.Today;
//            var tomorrow = today.AddDays(1);

//            foreach (var med in activeMedications)
//            {
//                var patients = await db.PatientMedications
//                    .Where(pm => pm.MedicationID == med.MedicationID)
//                    .Select(pm => pm.Patient)
//                    .ToListAsync();

//                foreach (var patient in patients)
//                {
//                    bool alreadyGenerated = await db.DoseLogs
//                        .AnyAsync(d => d.MedicationID == med.MedicationID
//                                       && d.PatientID == patient.PatientID
//                                       && d.ScheduledTime >= today
//                                       && d.ScheduledTime < tomorrow);

//                    if (!alreadyGenerated)
//                    {
//                        for (int i = 0; i < med.Frequency; i++)
//                        {
//                            db.DoseLogs.Add(new DoseLog
//                            {
//                                MedicationID = med.MedicationID,
//                                PatientID = patient.PatientID,
//                                Status = DoseStatus.Scheduled,
//                                ScheduledTime = today.AddHours(i * (24 / med.Frequency))
//                            });
//                        }
//                    }
//                }
//            }

//            await db.SaveChangesAsync();
//        }
//    }
//}
