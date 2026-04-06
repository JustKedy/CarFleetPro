using System;
using Microsoft.Maui.Controls;
using CarFleetPro.Mobile.ViewModels;

namespace CarFleetPro.Mobile.Views
{
    public partial class FleetManagementPage : ContentPage
    {
        public FleetManagementPage()
        {
            InitializeComponent();
            BindingContext = new FleetManagementViewModel();
        }

        public async void OnAddNewVehicleClicked(object sender, EventArgs e)
        {
            try
            {
                // TEST MESAJINI SİLDİK, GERÇEK SAYFAYA GEÇİŞİ EKLEDİK
                await Navigation.PushAsync(new AddNewVehiclePage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"YUNUS HATA: {ex.Message}");
            }
        }
    }
}