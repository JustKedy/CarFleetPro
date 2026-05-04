using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class InvoicePage : ContentPage
    {
        public InvoicePage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnViewInvoiceClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("PDF Yüklendi", "Fatura ekstresi başarılı bir şekilde oluşturuldu ve PDF okuyucuya iletildi.", "Kapat");
        }
    }
}
