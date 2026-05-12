using Microsoft.Maui.Controls;
using System;
using System.Linq;

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
            await LoadStaff();
            await CheckUserRole();
        }

        private async System.Threading.Tasks.Task CheckUserRole()
        {
            var profile = await _apiService.GetProfileAsync();
            if (profile != null && profile.Role == "Yönetici")
            {
                AddNewButton.IsVisible = true;
            }
            else
            {
                AddNewButton.IsVisible = false;
            }
        }

        private async System.Threading.Tasks.Task LoadStaff()
        {
            var staff = await _apiService.GetStaffAsync();
            // Ana yöneticiyi (Alper) listeden gizle
            StaffList.ItemsSource = staff?.Where(s => s.Email.ToLower() != "alper@carfleet.com").ToList();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnAddNewStaffClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdminRegistrationPage());
        }

        private async void OnEditStaffClicked(object? sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Models.StaffInfo staff)
            {
                await Navigation.PushAsync(new EditStaffPage(staff));
            }
        }

        private async void OnDeleteStaffClicked(object? sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is Models.StaffInfo staff)
            {
                bool answer = await DisplayAlertAsync("Sil", $"{staff.FullName} adlı personeli silmek istediğinize emin misiniz?", "Evet", "Hayır");
                if (answer)
                {
                    var (success, message) = await _apiService.DeleteStaffAsync(staff.Id);
                    if (success)
                    {
                        await LoadStaff();
                    }
                    else
                    {
                        await DisplayAlertAsync("Hata", message, "Tamam");
                    }
                }
            }
        }
    }
}
