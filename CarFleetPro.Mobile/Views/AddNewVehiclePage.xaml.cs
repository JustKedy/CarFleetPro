using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace CarFleetPro.Mobile.Views
{
    public partial class AddNewVehiclePage : ContentPage
    {
        private Vehicle? _duzenlenenArac;
        private readonly ApiService _apiService = new();

        public AddNewVehiclePage()
        {
            InitializeComponent();
        }

        public AddNewVehiclePage(Vehicle secilenArac)
        {
            InitializeComponent();
            _duzenlenenArac = secilenArac;
            SayfayiDuzenlemeModunaGecir();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            if (Navigation != null)
            {
                await Navigation.PopAsync();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriDoldur();
        }

        private async Task VerileriDoldur()
        {
            var brandsTask = _apiService.GetBrandsAsync();
            var colorsTask = _apiService.GetColorsAsync();
            var statusesTask = _apiService.GetStatusesAsync();

            await Task.WhenAll(brandsTask, colorsTask, statusesTask);

            BrandPicker.ItemsSource = brandsTask.Result;
            RenkPicker.ItemsSource = colorsTask.Result;
            DurumPicker.ItemsSource = statusesTask.Result;

            BrandPicker.SelectedIndexChanged += async (s, e) =>
            {
                ModelPicker.ItemsSource = null;
                if (BrandPicker.SelectedItem != null)
                {
                    var selectedBrand = BrandPicker.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(selectedBrand))
                        ModelPicker.ItemsSource = await _apiService.GetModelsAsync(selectedBrand);
                }
            };

            if (_duzenlenenArac != null)
            {
                BrandPicker.SelectedItem = _duzenlenenArac.Marka;
                DurumPicker.SelectedItem = _duzenlenenArac.Durum;
            }
        }

        private void SayfayiDuzenlemeModunaGecir()
        {
            PageTitleLabel.Text = "ARAÃ‡ DÃœZENLE";
            if (_duzenlenenArac != null)
            {
                PlakaEntry.Text = _duzenlenenArac.Plaka;
                KmEntry.Text = _duzenlenenArac.Km.ToString();
                HpEntry.Text = _duzenlenenArac.Hp.ToString();
                YilEntry.Text = (DateTime.Now.Year - _duzenlenenArac.Yas).ToString();
                DurumPicker.SelectedItem = _duzenlenenArac.Durum;
            }
        }

        public async void OnUploadImageTapped(object? sender, EventArgs e)
        {
            // Yeni araç eklerken araç ID'si henüz yok — önce kaydet sonra fotoğraf ekle
            if (_duzenlenenArac == null)
            {
                await DisplayAlertAsync(
                    "Bilgi",
                    "Fotoğraf eklemek için önce aracı kaydedin. Kayıt sonrası fotoğraf yükleme ekranına yönlendirileceksiniz.",
                    "Anladım");
                return;
            }

            // Düzenleme modunda: doğrudan galeri sayfasına git
            var images = await _apiService.GetVehicleImagesAsync(_duzenlenenArac.Id);
            await Navigation.PushAsync(new VehiclePhotoGalleryPage(_duzenlenenArac, images, true, _apiService));
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlakaEntry.Text))
            {
                await DisplayAlertAsync("Eksik Bilgi", "LÃ¼tfen araÃ§ plakasÄ±nÄ± girin.", "Tamam");
                return;
            }
            if (BrandPicker.SelectedItem == null)
            {
                await DisplayAlertAsync("Eksik Bilgi", "LÃ¼tfen bir marka seÃ§in.", "Tamam");
                return;
            }
            if (ModelPicker.SelectedItem == null)
            {
                await DisplayAlertAsync("Eksik Bilgi", "LÃ¼tfen bir model seÃ§in.", "Tamam");
                return;
            }
            if (string.IsNullOrWhiteSpace(YilEntry.Text) || !int.TryParse(YilEntry.Text, out int yil) || yil < 1950 || yil > DateTime.Now.Year)
            {
                await DisplayAlertAsync("HatalÄ± Bilgi", $"LÃ¼tfen geÃ§erli bir yÄ±l girin (1950 - {DateTime.Now.Year}).", "Tamam");
                return;
            }

            int durumId = 0; 
            var selectedDurum = DurumPicker.SelectedItem?.ToString();
            if (string.Equals(selectedDurum, "DOLU", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(selectedDurum, "KÄ°RADA", StringComparison.OrdinalIgnoreCase)) durumId = 1;
            else if (string.Equals(selectedDurum, "BAKIMDA", StringComparison.OrdinalIgnoreCase)) durumId = 2;

            var request = new CreateVehicleRequest
            {
                PlateNumber = PlakaEntry.Text.Trim().ToUpper(),
                Brand = BrandPicker.SelectedItem.ToString()!,
                Model = ModelPicker.SelectedItem.ToString()!,
                Year = yil,
                Mileage = int.TryParse(KmEntry.Text, out int km) ? km : 0,
                HorsePower = int.TryParse(HpEntry.Text, out int hp) ? hp : 0,
                Color = RenkPicker.SelectedItem?.ToString(),
                Branch = "Merkez Åube",
                Status = durumId
            };

            Button? saveBtn = sender as Button;
            if (saveBtn != null) saveBtn.IsEnabled = false;

            if (_duzenlenenArac != null)
            {
                var (success, message) = await _apiService.UpdateVehicleAsync(_duzenlenenArac.Id, request);
                if (saveBtn != null) saveBtn.IsEnabled = true;

                if (success)
                {
                    await ShowSuccessToast("AraÃ§ baÅŸarÄ±yla gÃ¼ncellendi!");
                    WeakReferenceMessenger.Default.Send(new VehicleAddedMessage());
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlertAsync("Hata", message, "Tamam");
                }
            }
            else
            {
                var (success, message) = await _apiService.CreateVehicleAsync(request);
                if (saveBtn != null) saveBtn.IsEnabled = true;

                if (success)
                {
                    await ShowSuccessToast("Araç filoya başarıyla eklendi!");
                    WeakReferenceMessenger.Default.Send(new VehicleAddedMessage());

                    // Araç eklendikten sonra fotoğraf eklemek ister misin?
                    var addPhoto = await DisplayAlert("Fotoğraf Ekle", "Araç başarıyla eklendi! Şimdi fotoğraf eklemek ister misiniz?", "Evet, Ekle", "Hayır");

                    if (addPhoto)
                    {
                        // Yeni eklenen aracın ID'sini al
                        var vehicles = await _apiService.GetVehiclesAsync(forceRefresh: true);
                        var newVehicle = vehicles.FirstOrDefault(v =>
                            string.Equals(v.Plaka, request.PlateNumber, StringComparison.OrdinalIgnoreCase));

                        if (newVehicle != null)
                        {
                            await Navigation.PushAsync(new VehiclePhotoGalleryPage(newVehicle, new System.Collections.Generic.List<VehicleImageInfo>(), true, _apiService));
                            return;
                        }
                    }

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", message, "Tamam");
                }
            }
        }

        private async Task ShowSuccessToast(string message)
        {
            if (Shell.Current?.CurrentPage == null) return;

            var toastGrid = new Border
            {
                BackgroundColor = Color.FromArgb("#10B981"),
                Padding = new Thickness(20, 15),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 0, 100),
                Opacity = 0,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 25 },
                Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 5), Radius = 10, Opacity = 0.3f },
                Content = new Label { Text = message, TextColor = Colors.White, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center }
            };

            var mainLayout = this.Content as Grid;
            if (mainLayout != null)
            {
                Grid.SetRowSpan(toastGrid, mainLayout.RowDefinitions.Count > 0 ? mainLayout.RowDefinitions.Count : 1);
                mainLayout.Children.Add(toastGrid);
                toastGrid.Scale = 0.8;

                await Task.WhenAll(toastGrid.FadeToAsync(1, 250, Easing.CubicOut), toastGrid.ScaleToAsync(1, 250, Easing.CubicOut));
                await Task.Delay(2000);
                await toastGrid.FadeToAsync(0, 300, Easing.CubicIn);

                mainLayout.Children.Remove(toastGrid);
            }
        }
    }
}
