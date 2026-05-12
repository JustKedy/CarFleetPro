using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.Views
{
    public partial class GaragePage : Microsoft.Maui.Controls.ContentPage
    {
        private readonly GarageViewModel _viewModel;

        public GaragePage(GarageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // SAYFA AÇILIŞ ANİMASYONU
            this.Opacity = 0;
            await this.FadeToAsync(1, 300, Microsoft.Maui.Easing.CubicOut);

            // Verileri yenileme komutu
            if (_viewModel.VerileriYenileCommand.CanExecute(null))
            {
                await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
            }
        }

        // --- KAYDIRMA (SLIDING) ANİMASYONU ---
        private async void OnAracTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Grid grid) return;

            // Tıklanan grid'in bağlı olduğu araç bilgisini al
            if (grid.BindingContext is not Vehicle secilenArac) return;

            // XAML'daki detay panelini (Border) bul (VerticalStackLayout'un 3. elemanı)
            if (grid.Parent is VerticalStackLayout parentLayout && parentLayout.Children.Count >= 3)
            {
                if (parentLayout.Children[2] is Border detayPaneli)
                {
                    secilenArac.IsExpanded = !secilenArac.IsExpanded;

                    if (secilenArac.IsExpanded)
                    {
                        detayPaneli.IsVisible = true;
                        detayPaneli.Opacity = 0;
                        detayPaneli.TranslationY = -20; // Yukarıdan süzülerek gelsin

                        await Task.WhenAll(
                            detayPaneli.FadeToAsync(1, 400, Microsoft.Maui.Easing.CubicOut),
                            detayPaneli.TranslateToAsync(0, 0, 400, Microsoft.Maui.Easing.CubicOut)
                        );
                    }
                    else
                    {
                        await Task.WhenAll(
                            detayPaneli.FadeToAsync(0, 350, Microsoft.Maui.Easing.CubicIn),
                            detayPaneli.TranslateToAsync(0, -20, 350, Microsoft.Maui.Easing.CubicIn)
                        );
                        detayPaneli.IsVisible = false;
                    }
                }
            }
        }

        private async void OnKiralaClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is Vehicle vehicle)
            {
                if (Navigation != null)
                {
                    await Navigation.PushAsync(new RentalFormPage(vehicle));
                }
            }
        }

        private async void OnBakimClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is Vehicle vehicle)
            {
                if (Navigation != null)
                {
                    await Navigation.PushAsync(new VehicleMaintenancePage(vehicle));
                }
            }
        }

        private void OnSegmentCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (BindingContext is GarageViewModel vm)
            {
                vm.SegmentToggledCommand.Execute(null);
            }
        }

    }
}
