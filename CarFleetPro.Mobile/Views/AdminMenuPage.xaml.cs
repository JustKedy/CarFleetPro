using System;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views
{
    public partial class AdminMenuPage : ContentPage
    {
        public AdminMenuPage()
        {
            InitializeComponent();
        }

        private async void OnInvoicesClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new InvoicePage());
        }

        private async void OnStaffClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new StaffManagementPage());
        }

        private async void OnSendNotificationClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new NotificationManagementPage());
        }

        private async void OnPricePolicyClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new PricePolicyPage());
        }
    }
}
