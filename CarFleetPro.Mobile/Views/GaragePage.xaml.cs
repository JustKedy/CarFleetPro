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

            
            this.Opacity = 0;
            await this.FadeToAsync(1, 300, Easing.CubicOut);

            
            await _viewModel.VerileriYenileCommand.ExecuteAsync(null);
        }

        
        
        
        
        private async void OnAracTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Grid grid) return;

            
            if (grid.BindingContext is not Vehicle secilenArac) return;

            
            
            if (grid.Parent is VerticalStackLayout parentLayout && parentLayout.Children.Count >= 3)
            {
                if (parentLayout.Children[2] is Border detayPaneli)
                {
                    
                    secilenArac.IsExpanded = !secilenArac.IsExpanded;

                    if (secilenArac.IsExpanded)
                    {
                        
                        detayPaneli.IsVisible = true;
                        detayPaneli.Opacity = 0;
                        detayPaneli.TranslationY = -20; 

                        await Task.WhenAll(
                            detayPaneli.FadeToAsync(1, 400, Easing.CubicOut),
                            detayPaneli.TranslateToAsync(0, 0, 400, Easing.CubicOut)
                        );
                    }
                    else
                    {
                        
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