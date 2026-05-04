using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class StaffManagementPage : ContentPage
    {
        public StaffManagementPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAddNewStaffClicked(object? sender, EventArgs e)
        {
            // YUNUS İÇİN NOT: Personel ekleme ekranına veya AdminRegistrationPage'e yönlendir.
            await Navigation.PushAsync(new AdminRegistrationPage());
        }
    }
}
