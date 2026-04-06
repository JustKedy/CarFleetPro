using System;
using Microsoft.Maui.Controls;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views
{
    public partial class FleetManagementPage : ContentPage
    {
        private readonly FleetManagementViewModel _viewModel;

        public FleetManagementPage()
        {
            InitializeComponent();
            _viewModel = new FleetManagementViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // AddNewVehiclePage araç ekleyince bu mesajı alıp listeyi yenile
            WeakReferenceMessenger.Default.Register<VehicleAddedMessage>(this, async (recipient, message) =>
            {
                await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Bellek sızıntısını önlemek için aboneliği iptal et
            WeakReferenceMessenger.Default.Unregister<VehicleAddedMessage>(this);
        }

        public async void OnAddNewVehicleClicked(object? sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new AddNewVehiclePage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"YUNUS HATA: {ex.Message}");
            }
        }
    }
}