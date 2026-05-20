using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class InvoicePage : ContentPage
    {
        private readonly Services.ApiService _apiService;
        private List<Models.InvoiceInfo> _allInvoices = new();
        private bool _isFiltered = false;

        public InvoicePage()
        {
            InitializeComponent();
            _apiService = new Services.ApiService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadInvoices();
        }

        private async System.Threading.Tasks.Task LoadInvoices()
        {
            _allInvoices = await _apiService.GetInvoicesAsync();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var query = SearchEntry.Text?.Trim().ToLower() ?? string.Empty;
            var filtered = _allInvoices.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(i => 
                    i.CustomerName.ToLower().Contains(query) || 
                    $"INV-{i.InvoiceId}".ToLower().Contains(query));
            }

            if (_isFiltered)
            {
                // Ödenmemiş/Bekleyen faturaları filtrele
                filtered = filtered.Where(i => i.Status != "ÖDENDİ");
            }

            InvoiceList.ItemsSource = filtered.ToList();
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void OnFilterClicked(object? sender, EventArgs e)
        {
            _isFiltered = !_isFiltered;
            FilterButton.Text = _isFiltered ? "BEKLEYEN" : "TÜMÜ";
            bool isDark = Application.Current?.UserAppTheme == AppTheme.Dark;
            FilterButton.TextColor = _isFiltered ? Colors.White : (isDark ? Color.FromArgb("#F1F5F9") : Color.FromArgb("#1F2937"));
            FilterButton.BackgroundColor = _isFiltered ? Color.FromArgb("#3B82F6") : (isDark ? Color.FromArgb("#334155") : Color.FromArgb("#E5E7EB"));
            
            ApplyFilter();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnDownloadPdfClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Models.InvoiceInfo invoice)
            {
                try
                {
                    var pdfUrl = $"https://carfleetpro-hcf2f6hua6f2h5f0.westeurope-01.azurewebsites.net/api/Invoice/{invoice.InvoiceId}/pdf";
                    
                    // Geçici bir MAUI dosyası açmak yerine direkt URL'yi tarayıcıda açmak en güvenli/kolay yoldur
                    // Eğer Android izin verirse direkt PDF görüntüleyiciyi tetikler.
                    await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(new Uri(pdfUrl));
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Hata", $"PDF açılamadı: {ex.Message}", "Tamam");
                }
            }
        }
    }
}
