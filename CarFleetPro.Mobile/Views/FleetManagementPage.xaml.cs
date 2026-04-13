using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views
{
    public partial class FleetManagementPage : ContentPage
    {
        private readonly FleetManagementViewModel _viewModel;

        // DI: Singleton VM ile liste state'ini korur
        public FleetManagementPage(FleetManagementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel     = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // SAYFA AÇILIŞ ANİMASYONU
            this.Opacity = 0;
            await this.FadeToAsync(1, 300, Easing.CubicOut);

            // Eski kodun
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

        // ==========================================================
        // KESİN ÇÖZÜM: MAUI BindableLayout Hata Giderme
        // Görsel ağaçtan (XAML) bağlamak yerine, Border'ın kendi arka 
        // planındaki nesneyi C# ile ViewModel'e yönlendiriyoruz.
        // ==========================================================
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
    }
}