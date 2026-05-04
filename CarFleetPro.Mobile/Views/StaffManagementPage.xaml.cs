using Microsoft.Maui.Controls;
using System;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views
{
    public partial class StaffManagementPage : ContentPage
    {
        private readonly Services.ApiService _apiService;

        public StaffManagementPage()
        {
            InitializeComponent();
            _apiService = new Services.ApiService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadStaffAsync();
        }

        private async System.Threading.Tasks.Task LoadStaffAsync()
        {
            var users = await _apiService.GetUsersAsync();
            StaffList.ItemsSource = users;
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
