using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views
{
    public partial class PricePolicyPage : ContentPage
    {
        private readonly ApiService _apiService = new();
        private List<PricePolicy> _allPolicies = new();
        private List<Vehicle> _allVehicles = new();

        public PricePolicyPage()
        {
            InitializeComponent();
            VehicleGroupPicker.SelectedIndexChanged += OnGroupSelectionChanged;
            VehiclePicker.SelectedIndexChanged += OnVehicleSelectionChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            _allPolicies = await _apiService.GetPricePoliciesAsync();
            _allVehicles = await _apiService.GetVehiclesAsync();
            VehiclePicker.ItemsSource = _allVehicles;
            
            // Başlangıçta Global politikayı yükle
            UpdateFields("Global", "All");
        }

        private void OnGroupSelectionChanged(object? sender, EventArgs e)
        {
            var selected = VehicleGroupPicker.SelectedItem?.ToString();
            SpecificVehicleLayout.IsVisible = selected == "Araca Özel";
            
            if (selected == "Araca Özel")
            {
                BasePriceEntry.Text = "";
                MaxDiscountEntry.Text = "";
            }
            else
            {
                string type = selected == "Tüm Araçlar" ? "Global" : "Segment";
                string value = selected == "Tüm Araçlar" ? "All" : selected!;
                UpdateFields(type, value);
            }
        }

        private void UpdateFields(string type, string value)
        {
            var policy = _allPolicies.FirstOrDefault(p => p.TargetType == type && p.TargetValue == value);
            if (policy != null)
            {
                BasePriceEntry.Text = policy.BasePrice.ToString();
                MaxDiscountEntry.Text = policy.MaxDiscountPercentage.ToString();
            }
            else
            {
                BasePriceEntry.Text = "";
                MaxDiscountEntry.Text = "";
            }
        }

        private void OnVehicleSelectionChanged(object? sender, EventArgs e)
        {
            if (VehiclePicker.SelectedItem is not Vehicle selectedVehicle) return;
            // Plaka bazlı mevcut politikayı yükle
            UpdateFields("Vehicle", selectedVehicle.Plaka);
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnUpdateClicked(object? sender, EventArgs e)
        {
            var selectedGroup = VehicleGroupPicker.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedGroup)) return;

            string basePriceText = BasePriceEntry.Text?.Replace(",", ".") ?? "0";
            string maxDiscountText = MaxDiscountEntry.Text?.Replace(",", ".") ?? "0";

            decimal basePrice = decimal.TryParse(basePriceText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal bp) ? bp : 0;
            double maxDiscount = double.TryParse(maxDiscountText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double md) ? md : 0;

            if (selectedGroup == "Araca Özel")
            {
                var selectedVehicle = VehiclePicker.SelectedItem as Vehicle;
                if (selectedVehicle == null)
                {
                    await DisplayAlertAsync("Hata", "Lütfen bir araç seçin.", "Tamam");
                    return;
                }

                // Araç özel politikayı PricePolicies tablosuna kaydet (plaka ile)
                var policy = new PricePolicy
                {
                    TargetType = "Vehicle",
                    TargetValue = selectedVehicle.Plaka,
                    BasePrice = basePrice,
                    MaxDiscountPercentage = maxDiscount
                };

                var (success, message) = await _apiService.SavePricePolicyAsync(policy);
                if (success)
                {
                    await DisplayAlertAsync("Başarılı", $"{selectedVehicle.Plaka} plakalı araç için özel fiyat tanımlandı.", "Tamam");
                    await Navigation.PopAsync();
                }
                else await DisplayAlertAsync("Hata", message, "Tamam");
            }
            else
            {
                // Global veya Segment bazlı güncelleme
                var policy = new PricePolicy
                {
                    TargetType = selectedGroup == "Tüm Araçlar" ? "Global" : "Segment",
                    TargetValue = selectedGroup == "Tüm Araçlar" ? "All" : selectedGroup,
                    BasePrice = basePrice,
                    MaxDiscountPercentage = maxDiscount
                };

                var (success, message) = await _apiService.SavePricePolicyAsync(policy);
                if (success)
                {
                    await DisplayAlertAsync("Başarılı", "Fiyat politikası güncellendi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else await DisplayAlertAsync("Hata", message, "Tamam");
            }
        }
    }
}

