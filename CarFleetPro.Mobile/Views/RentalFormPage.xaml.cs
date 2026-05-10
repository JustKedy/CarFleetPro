using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

namespace CarFleetPro.Mobile.Views
{
    public partial class RentalFormPage : Microsoft.Maui.Controls.ContentPage
    {
        private readonly ApiService _apiService = new();
        private readonly Vehicle? _vehicle;

        public RentalFormPage(Vehicle vehicle)
        {
            InitializeComponent();
            _vehicle = vehicle;
            BindingContext = _vehicle;

            if (!string.IsNullOrEmpty(vehicle.ResimUrl))
            {
                ImagesCarousel.ItemsSource = new List<string> { vehicle.ResimUrl };
            }
        }

        public RentalFormPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Sayfa açılırken yumuşak bir geçiş efekti
            this.Opacity = 0;
            await this.FadeToAsync(1, 400, Easing.CubicOut);
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnCompleteRentalClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("Bilgi", "Kiralama işlemi başarılı bir şekilde kaydedildi.", "Tamam");
            await Navigation.PopAsync();
        }

        private void OnPrevImageClicked(object? sender, EventArgs e)
        {
            if (ImagesCarousel.ItemsSource is IList<string> items && items.Count > 0)
            {
                int currentIndex = ImagesCarousel.Position;
                if (currentIndex > 0)
                {
                    ImagesCarousel.Position = currentIndex - 1;
                }
                else
                {
                    // Loop to the end
                    ImagesCarousel.Position = items.Count - 1;
                }
            }
        }

        private void OnNextImageClicked(object? sender, EventArgs e)
        {
            if (ImagesCarousel.ItemsSource is IList<string> items && items.Count > 0)
            {
                int currentIndex = ImagesCarousel.Position;
                if (currentIndex < items.Count - 1)
                {
                    ImagesCarousel.Position = currentIndex + 1;
                }
                else
                {
                    // Loop to the beginning
                    ImagesCarousel.Position = 0;
                }
            }
        }
    }
}
