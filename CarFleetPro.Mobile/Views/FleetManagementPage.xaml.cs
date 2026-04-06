using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using CarFleetPro.Mobile.ViewModels;

namespace CarFleetPro.Mobile.Views
{
    public partial class FleetManagementPage : ContentPage
    {
        public FleetManagementPage()
        {
            InitializeComponent();

            // XAML arayüzüne aklını (ViewModel'i) bağlıyoruz
            BindingContext = new FleetManagementViewModel();
        }

        // DİKKAT: MAUIX hatasını engellemek için public yapıldı!
        public async void OnAddNewVehicleClicked(object sender, EventArgs e)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    // Şimdilik test mesajı, Kadir'in efsane formu bitince buraya yönlendirme kodu yazacağız.
                    await Application.Current.MainPage.DisplayAlert("Rota Hazırlanıyor", "Yakında Kadir'in Araç Ekleme formuna geçiş yapılacak!", "Anlaşıldı");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"YUNUS HATA: Yeni araç ekle butonunda sorun var: {ex.Message}");
            }
        }
    }
}