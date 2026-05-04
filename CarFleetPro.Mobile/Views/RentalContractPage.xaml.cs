using Microsoft.Maui.Controls;
using System;

namespace CarFleetPro.Mobile.Views
{
    public partial class RentalContractPage : ContentPage
    {
        public RentalContractPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
