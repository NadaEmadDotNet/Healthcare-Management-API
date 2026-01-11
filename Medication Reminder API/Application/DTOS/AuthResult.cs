namespace Medication_Reminder_API.Application.DTOS
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
    }
}
