using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class InvoicePage : ContentPage
    {
        private readonly Services.ApiService _apiService;

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
            var invoices = await _apiService.GetInvoicesAsync();
            InvoiceList.ItemsSource = invoices;
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
