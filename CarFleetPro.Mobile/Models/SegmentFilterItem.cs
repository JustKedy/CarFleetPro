using CommunityToolkit.Mvvm.ComponentModel;

namespace CarFleetPro.Mobile.Models
{
    public partial class SegmentFilterItem : ObservableObject
    {
        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsSelected { get; set; } = false;
    }
}
