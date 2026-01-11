using Medication_Reminder_API.Application.Interfaces;
using Medication_Reminder_API.Domain.Models;
using Microsoft.EntityFrameworkCore;

public class DoseLogRepository : IDoseLogRepository
{
    private readonly ApplicationDbContext _context;

    public DoseLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<DoseLog> GetAll() => _context.DoseLogs.AsNoTracking();

    public async Task<DoseLog?> GetByIdAsync(int id) =>
        await _context.DoseLogs
            .Include(d => d.Medication)
            .Include(d => d.Patient)
            .FirstOrDefaultAsync(d => d.DoseLogID == id);

    public async Task<List<DoseLog>> GetByPatientIdAsync(int patientId) =>
        await _context.DoseLogs
            .Include(d => d.Medication)
            .Where(d => d.PatientID == patientId)
            .ToListAsync();

    public async Task AddAsync(DoseLog dose) => await _context.DoseLogs.AddAsync(dose);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
