using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class NotificationManagementPage : ContentPage
    {
        public NotificationManagementPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSendNotificationClicked(object? sender, EventArgs e)
        {
            if(TargetAudiencePicker.SelectedItem == null)
            {
                await DisplayAlertAsync("Eksik", "Lütfen hedef kitle seçin.", "Tamam");
                return;
            }

            if(string.IsNullOrEmpty(TitleEntry.Text) || string.IsNullOrEmpty(MessageEditor.Text))
            {
                await DisplayAlertAsync("Eksik", "Lütfen başlık ve mesaj içeriğini doldurun.", "Tamam");
                return;
            }

            // Burada normalde NotificationDto oluşturup API'ye POST edeceğiz.
            // await _apiService.SendNotificationAsync(dto);

            await DisplayAlertAsync("Başarılı", $"{TargetAudiencePicker.SelectedItem} grubuna bildirim başarıyla iletildi.", "Tamam");
            
            // Temizle
            TitleEntry.Text = string.Empty;
            MessageEditor.Text = string.Empty;
            SendSmsCheckbox.IsChecked = false;
        }
    }
}
