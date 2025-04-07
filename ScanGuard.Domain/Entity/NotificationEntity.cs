namespace ScanGuard.Domain.Entity
{
    public class NotificationEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NotificationTitle { get; set; }
        public string NotificationContent { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime SentDateTime { get; set; } = DateTime.Now;
    }
}
