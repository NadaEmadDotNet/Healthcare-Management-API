namespace Medication_Reminder_API.Application.DTOS
{
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }
    }
    public class ServiceResult
    {
        public bool Success { get; set; }    
        public string Message { get; set; } = string.Empty;
    }
}
