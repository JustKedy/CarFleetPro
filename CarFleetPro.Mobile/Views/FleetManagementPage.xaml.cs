using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views
{
    public partial class FleetManagementPage : ContentPage
    {
        private readonly FleetManagementViewModel _viewModel;

        
        public FleetManagementPage(FleetManagementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel     = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            
            this.Opacity = 0;
            await this.FadeToAsync(1, 300, Easing.CubicOut);

            
            await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
        }

        public async void OnAddNewVehicleClicked(object? sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new AddNewVehiclePage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HATA: {ex.Message}");
            }
        }

        
        
        
        
        
        private void OnDuzenleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject bindable && bindable.BindingContext is Vehicle secilenArac)
            {
                _viewModel.DuzenleCommand.Execute(secilenArac);
            }
        }

        private void OnSilTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject bindable && bindable.BindingContext is Vehicle secilenArac)
            {
                _viewModel.SilCommand.Execute(secilenArac);
            }
        }

        private async void OnBakimKayitlariClicked(object? sender, EventArgs e)
        {
            if (sender is BindableObject bindable && bindable.BindingContext is Vehicle secilenArac)
            {
                if (Navigation != null)
                {
                    await Navigation.PushAsync(new VehicleMaintenancePage());
                }
            }
        }

        private async void OnHasarKayitlariClicked(object? sender, EventArgs e)
        {
            if (sender is BindableObject bindable && bindable.BindingContext is Vehicle secilenArac)
            {
                if (Navigation != null)
                {
                    await Navigation.PushAsync(new DamageRecordPage());
                }
            }
        }
    }
}