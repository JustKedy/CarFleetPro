using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class NotificationManagementPage : ContentPage
    {
        private readonly Services.ApiService _apiService;

        public NotificationManagementPage()
        {
            InitializeComponent();
            _apiService = new Services.ApiService();
        }

        private async void OnSendNotificationClicked(object? sender, EventArgs e)
        {
            var targetAudience = TargetAudiencePicker.SelectedItem as string;
            var title = TitleEntry.Text?.Trim();
            var message = MessageEditor.Text?.Trim();

            if (string.IsNullOrEmpty(targetAudience) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
            {
                await DisplayAlertAsync("Uyarı", "Lütfen tüm alanları doldurun.", "Tamam");
                return;
            }

            var request = new CarFleetPro.Mobile.Models.SendNotificationRequest
            {
                Title = title,
                Message = message,
                Type = targetAudience
            };

            var (success, resultMessage) = await _apiService.SendNotificationAsync(request);

            if (success)
            {
                await DisplayAlertAsync("Başarılı", "Bildirim başarıyla gönderildi.", "Tamam");
                TitleEntry.Text = string.Empty;
                MessageEditor.Text = string.Empty;
                TargetAudiencePicker.SelectedItem = null;
                SendSmsCheckbox.IsChecked = false;
            }
            else
            {
                await DisplayAlertAsync("Hata", resultMessage, "Tamam");
            }
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
