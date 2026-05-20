using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class RentalContractPage : ContentPage
    {
        private readonly Services.ApiService _apiService;
        private List<Models.RentalInfo> _allRentals = new();
        private bool _isFiltered = false;

        public RentalContractPage()
        {
            InitializeComponent();
            _apiService = new Services.ApiService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRentals();
        }

        private async System.Threading.Tasks.Task LoadRentals()
        {
            _allRentals = await _apiService.GetRentalsAsync();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var query = SearchEntry.Text?.Trim().ToLower() ?? string.Empty;
            var filtered = _allRentals.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(r => 
                    r.CustomerName.ToLower().Contains(query) || 
                    r.VehiclePlate.ToLower().Contains(query));
            }

            if (_isFiltered)
            {
                // Sadece Aktif sözleşmeleri filtrele
                filtered = filtered.Where(r => r.Status == "Aktif");
            }

            ContractsList.ItemsSource = filtered.ToList();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void OnFilterClicked(object? sender, EventArgs e)
        {
            _isFiltered = !_isFiltered;
            FilterButton.Text = _isFiltered ? "AKTİF" : "TÜMÜ";
            bool isDark = Application.Current?.UserAppTheme == AppTheme.Dark;
            FilterButton.TextColor = _isFiltered ? Colors.White : (isDark ? Color.FromArgb("#F1F5F9") : Color.FromArgb("#1F2937"));
            FilterButton.BackgroundColor = _isFiltered ? Color.FromArgb("#3B82F6") : (isDark ? Color.FromArgb("#334155") : Color.FromArgb("#E5E7EB"));
            
            ApplyFilter();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
