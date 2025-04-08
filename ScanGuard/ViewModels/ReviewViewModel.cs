using ScanGuard.Domain.Entity;

namespace ScanGuard.ViewModels
{
    public class ReviewViewModel
    {
        public bool CanAdd { get; set; }
        public bool IsGood { get; set; }
        public List<ReviewEntity> Reviews { get; set; }
    }
}
