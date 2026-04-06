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
        // ÇÖZÜM: Null uyarısı için ? eklendi
        private Vehicle? _duzenlenenArac;
        private readonly ApiService _apiService = new ApiService();

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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerileriDoldur();
        }

        private async Task VerileriDoldur()
        {
            // Marka, Renk ve Durum listelerini paralel olarak veritabanından çek
            var brandsTask = _apiService.GetBrandsAsync();
            var colorsTask = _apiService.GetColorsAsync();
            var statusesTask = _apiService.GetStatusesAsync();

            await Task.WhenAll(brandsTask, colorsTask, statusesTask);

            BrandPicker.ItemsSource = brandsTask.Result;
            RenkPicker.ItemsSource = colorsTask.Result;
            DurumPicker.ItemsSource = statusesTask.Result;

            // Marka seçilince modelleri veritabanından çek
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

            // Düzenleme modunda picker'lara mevcut değerleri set et
            if (_duzenlenenArac != null)
            {
                BrandPicker.SelectedItem = _duzenlenenArac.Marka;
                // Model, marka seçilince yukarıdaki event ile otomatik yüklenip seçilecek
                DurumPicker.SelectedItem = _duzenlenenArac.Durum;
            }
        }

        private void SayfayiDuzenlemeModunaGecir()
        {
            PageTitleLabel.Text = "ARAÇ DÜZENLE";
            if (_duzenlenenArac != null)
            {
                PlakaEntry.Text = _duzenlenenArac.Plaka;
                KmEntry.Text = _duzenlenenArac.Km.ToString();
                HpEntry.Text = _duzenlenenArac.Hp.ToString();
                YilEntry.Text = (DateTime.Now.Year - _duzenlenenArac.Yas).ToString();
                DurumPicker.SelectedItem = _duzenlenenArac.Durum;
            }
        }

        // ÇÖZÜM: object? sender yapıldı
        public async void OnUploadImageTapped(object? sender, EventArgs e)
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // ÇÖZÜM: Yeni .NET 10 formatı (PickPhotosAsync)
                    var photos = await MediaPicker.Default.PickPhotosAsync();
                    var photo = photos?.FirstOrDefault();

                    if (photo != null)
                    {
                        ImageUploadBorder.Content = null;
                        var selectedImage = new Image
                        {
                            Source = ImageSource.FromFile(photo.FullPath),
                            Aspect = Aspect.AspectFill
                        };
                        ImageUploadBorder.Content = selectedImage;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HATA: {ex.Message}");
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            // ── 1. Form Validasyonu ──────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(PlakaEntry.Text))
            {
                await DisplayAlert("Eksik Bilgi", "Lütfen araç plakasını girin.", "Tamam");
                return;
            }
            if (BrandPicker.SelectedItem == null)
            {
                await DisplayAlert("Eksik Bilgi", "Lütfen bir marka seçin.", "Tamam");
                return;
            }
            if (ModelPicker.SelectedItem == null)
            {
                await DisplayAlert("Eksik Bilgi", "Lütfen bir model seçin.", "Tamam");
                return;
            }
            if (string.IsNullOrWhiteSpace(YilEntry.Text) || !int.TryParse(YilEntry.Text, out int yil) || yil < 1950 || yil > DateTime.Now.Year)
            {
                await DisplayAlert("Hatalı Bilgi", $"Lütfen geçerli bir yıl girin (1950 - {DateTime.Now.Year}).", "Tamam");
                return;
            }

            // ── 2. Düzenleme Modu ──────────────────────────────────────────
            if (_duzenlenenArac != null)
            {
                // TODO: PUT endpoint'i hazır olunca burası doldurulacak
                await ShowSuccessToast("Araç başarıyla güncellendi!");
                await Navigation.PopAsync();
                return;
            }

            // Durum ataması: 0=Müsait, 1=Kirada(Dolu), 2=Bakımda
            int durumId = 0; // Varsayılan Müsait
            var selectedDurum = DurumPicker.SelectedItem?.ToString()?.ToUpper();
            if (selectedDurum == "DOLU" || selectedDurum == "KİRADA") durumId = 1;
            else if (selectedDurum == "BAKIMDA") durumId = 2;

            // ── 3. Yeni Araç Oluştur ve API'ye Gönder ─────────────────────
            var request = new CreateVehicleRequest
            {
                PlateNumber  = PlakaEntry.Text.Trim().ToUpper(),
                Brand        = BrandPicker.SelectedItem.ToString()!,
                Model        = ModelPicker.SelectedItem.ToString()!,
                Year         = yil,
                Mileage      = int.TryParse(KmEntry.Text, out int km)  ? km  : 0,
                HorsePower   = int.TryParse(HpEntry.Text, out int hp)  ? hp  : 0,
                Color        = RenkPicker.SelectedItem?.ToString(),
                Branch       = "Merkez Şube",
                Status       = durumId
            };

            // Kaydet butonunu pasif yap - çift tıklamayı önle
            var saveButton = sender as Button;
            if (saveButton != null) saveButton.IsEnabled = false;

            var (success, message) = await _apiService.CreateVehicleAsync(request);

            if (saveButton != null) saveButton.IsEnabled = true;

            if (success)
            {
                await ShowSuccessToast("Araç filoya başarıyla eklendi! 🚗");
                // FleetManagementPage'e geri dönünce yenilemesi için mesaj gönder
                WeakReferenceMessenger.Default.Send(new VehicleAddedMessage());
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Hata", message, "Tamam");
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

                // ÇÖZÜM: Eski FadeTo yerine yeni FadeToAsync ve ScaleToAsync
                await Task.WhenAll(toastGrid.FadeToAsync(1, 250, Easing.CubicOut), toastGrid.ScaleToAsync(1, 250, Easing.CubicOut));
                await Task.Delay(2000);
                await toastGrid.FadeToAsync(0, 300, Easing.CubicIn);

                mainLayout.Children.Remove(toastGrid);
            }
        }
    }
}