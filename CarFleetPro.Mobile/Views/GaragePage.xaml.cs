using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.Views
{
    public partial class GaragePage : ContentPage
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
            await this.FadeTo(1, 300, Easing.CubicOut);

            // Eski kodun
            await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
        }

        // --- KADİR'İN LİSTESİ: KAYDIRMA (SLIDING) ANİMASYONU ---
        // XAML'da Grid.GestureRecognizers içindeki Command'i silip, 
        // yerine Tapped="OnAracTapped" yazarak bu metoda bağlaman gerekecek.
        // --- KADİR'İN LİSTESİ: KAYDIRMA (SLIDING) ANİMASYONU ---
        private async void OnAracTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Grid grid) return;

            // Tıklanan grid'in bağlı olduğu Vehicle modelini al
            if (grid.BindingContext is not Vehicle secilenArac) return;

            // XAML'da detayların olduğu Border'ı bulmamız lazım. 
            // Grid'in bir üst elemanı (VerticalStackLayout), onun da içindeki 3. eleman (Border)
            if (grid.Parent is VerticalStackLayout parentLayout && parentLayout.Children.Count >= 3)
            {
                if (parentLayout.Children[2] is Border detayPaneli)
                {
                    // DURUM TERSİNE ÇEVRİLİYOR
                    secilenArac.IsExpanded = !secilenArac.IsExpanded;

                    if (secilenArac.IsExpanded)
                    {
                        // AÇILIŞ ANİMASYONU (SÜRELER UZATILDI - 400ms)
                        detayPaneli.IsVisible = true;
                        detayPaneli.Opacity = 0;
                        detayPaneli.TranslationY = -20; // Hafif yukarıdan başla

                        await Task.WhenAll(
                            detayPaneli.FadeToAsync(1, 400, Easing.CubicOut),
                            detayPaneli.TranslateToAsync(0, 0, 400, Easing.CubicOut)
                        );
                    }
                    else
                    {
                        // KAPANIŞ ANİMASYONU (SÜRELER UZATILDI - 350ms)
                        await Task.WhenAll(
                            detayPaneli.FadeToAsync(0, 350, Easing.CubicIn),
                            detayPaneli.TranslateToAsync(0, -20, 350, Easing.CubicIn)
                        );

                        detayPaneli.IsVisible = false;
                    }
                }
            }
        }
    }
}