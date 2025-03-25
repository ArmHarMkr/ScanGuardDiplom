namespace ScanGuard.Domain.Entity
{
    public class SiteScanCountEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Url { get; set; }
        public int? CheckCount { get; set; } = 0;
    }   
}
