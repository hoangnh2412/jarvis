namespace Infrastructure.Notification.Email
{
    public enum EmailStatus
    {
        Queue = 1,
        Sending = 2,
        Success = 3,
        Error = 4,
        Retry = 5
    }
}