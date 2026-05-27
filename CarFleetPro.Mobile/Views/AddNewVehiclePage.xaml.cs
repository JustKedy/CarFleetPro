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
        private readonly System.Collections.Generic.List<FileResult> _tempPhotos = new();

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
            var typesTask = _apiService.GetCarTypesAsync();
            var statusesTask = _apiService.GetStatusesAsync();

            await Task.WhenAll(brandsTask, colorsTask, typesTask, statusesTask);

            BrandPicker.ItemsSource = brandsTask.Result;
            RenkPicker.ItemsSource = colorsTask.Result;
            SegmentPicker.ItemsSource = typesTask.Result;
            DurumPicker.ItemsSource = statusesTask.Result;

            BrandPicker.SelectedIndexChanged += async (s, e) =>
            {
                ModelPicker.ItemsSource = null;
                if (BrandPicker.SelectedItem is LookupItem selectedBrand)
                {
                    ModelPicker.ItemsSource = await _apiService.GetModelsAsync(selectedBrand.Id);
                }
            };

            if (_duzenlenenArac != null)
            {
            }
        }

        private void SayfayiDuzenlemeModunaGecir()
        {
            PageTitleLabel.Text = "ARAÇ DÜZENLE";
            if (_duzenlenenArac != null)
            {
                PlakaEntry.Text = _duzenlenenArac.Plaka;
                KmEntry.Text = _duzenlenenArac.Km.ToString();
                YilEntry.Text = (DateTime.Now.Year - _duzenlenenArac.Yas).ToString();
                BasePriceEntry.Text = _duzenlenenArac.BasePrice > 0 ? _duzenlenenArac.BasePrice.ToString() : "";
            }
        }

        public async void OnUploadImageTapped(object? sender, EventArgs e)
        {
            if (_duzenlenenArac == null)
            {
                try
                {
                    var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
                    {
                        PickerTitle = "Araç Fotoğraflarını Seçin",
                        FileTypes = FilePickerFileType.Images
                    });

                    if (results != null)
                    {
                        var validPhotos = results.Where(r => r != null).Cast<FileResult>().ToList();
                        if (validPhotos.Any())
                        {
                            _tempPhotos.Clear();
                            _tempPhotos.AddRange(validPhotos);

                            var firstPhoto = validPhotos.First();
                            UploadedImagePreview.Source = ImageSource.FromFile(firstPhoto.FullPath);
                            UploadedImagePreview.IsVisible = true;
                            DefaultUploadLayout.IsVisible = false;
                            UploadInstructionLabel.Text = $"{validPhotos.Count} Görsel Seçildi\n(Değiştirmek için tıklayın)";
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Hata", $"Fotoğraf seçilemedi: {ex.Message}", "Tamam");
                }
                return;
            }

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
                await DisplayAlertAsync("Eksik Bilgi", "Lütfen araç plakasını girin.", "Tamam");
                return;
            }
            if (BrandPicker.SelectedItem is not LookupItem selectedBrand)
            {
                await DisplayAlertAsync("Eksik Bilgi", "Lütfen bir marka seçin.", "Tamam");
                return;
            }
            if (ModelPicker.SelectedItem is not LookupItem selectedModel)
            {
                await DisplayAlertAsync("Eksik Bilgi", "Lütfen bir model seçin.", "Tamam");
                return;
            }
            if (SegmentPicker.SelectedItem is not LookupItem selectedSegment)
            {
                await DisplayAlertAsync("Eksik Bilgi", "Lütfen bir araç tipi seçin.", "Tamam");
                return;
            }
            if (string.IsNullOrWhiteSpace(YilEntry.Text) || !int.TryParse(YilEntry.Text, out int yil) || yil < 1950 || yil > DateTime.Now.Year)
            {
                await DisplayAlertAsync("Hatalı Bilgi", $"Lütfen geçerli bir yıl girin (1950 - {DateTime.Now.Year}).", "Tamam");
                return;
            }

            int durumId = 0; 
            var selectedDurum = DurumPicker.SelectedItem?.ToString();
            if (string.Equals(selectedDurum, "DOLU", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(selectedDurum, "KİRADA", StringComparison.OrdinalIgnoreCase)) durumId = 1;
            else if (string.Equals(selectedDurum, "BAKIMDA", StringComparison.OrdinalIgnoreCase)) durumId = 2;

            var request = new CreateVehicleRequest
            {
                PlateNumber = PlakaEntry.Text.Trim().ToUpper(),
                BrandId = selectedBrand.Id,
                ModelId = selectedModel.Id,
                Year = yil,
                Mileage = int.TryParse(KmEntry.Text, out int km) ? km : 0,
                HorsePower = 0,
                ColorId = (RenkPicker.SelectedItem as LookupItem)?.Id,
                Branch = "Merkez Şube",
                Status = durumId,
                SegmentId = selectedSegment.Id,
                BasePrice = decimal.TryParse(BasePriceEntry.Text, out decimal bp) ? bp : 0
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
                    WeakReferenceMessenger.Default.Send(new VehicleAddedMessage());

                    var vehicles = await _apiService.GetVehiclesAsync(forceRefresh: true);
                    var newVehicle = vehicles.FirstOrDefault(v =>
                        string.Equals(v.Plaka, request.PlateNumber, StringComparison.OrdinalIgnoreCase));

                    if (newVehicle != null && _tempPhotos.Any())
                    {
                        int totalCount = _tempPhotos.Count;
                        int uploadedCount = 0;
                        int failedCount = 0;

                        if (SubmitButton != null)
                        {
                            SubmitButton.IsEnabled = false;
                            SubmitButton.Text = $"Görseller Yükleniyor (0/{totalCount})...";
                        }

                        foreach (var photo in _tempPhotos)
                        {
                            uploadedCount++;
                            if (SubmitButton != null)
                            {
                                SubmitButton.Text = $"Görseller Yükleniyor ({uploadedCount}/{totalCount})...";
                            }

                            try
                            {
                                var (imgSuccess, _, _) = await _apiService.UploadVehicleImageAsync(
                                    newVehicle.Id,
                                    photo.FullPath,
                                    photo.FileName,
                                    photo.ContentType ?? "image/jpeg");

                                if (!imgSuccess) failedCount++;
                            }
                            catch
                            {
                                failedCount++;
                            }
                        }

                        if (SubmitButton != null)
                        {
                            SubmitButton.IsEnabled = true;
                            SubmitButton.Text = "ARACI KAYDET";
                        }

                        if (failedCount > 0)
                        {
                            await DisplayAlertAsync("Yükleme Sonucu", $"{totalCount} görselden {totalCount - failedCount} tanesi yüklendi. {failedCount} tanesi yüklenemedi.", "Tamam");
                        }
                        else
                        {
                            await ShowSuccessToast("Araç ve görseller başarıyla eklendi!");
                        }
                    }
                    else if (newVehicle != null)
                    {
                        var addPhoto = await DisplayAlertAsync("Fotoğraf Ekle", "Araç başarıyla eklendi! Şimdi fotoğraf eklemek ister misiniz?", "Evet, Ekle", "Hayır");

                        if (addPhoto)
                        {
                            await Navigation.PushAsync(new VehiclePhotoGalleryPage(newVehicle, new System.Collections.Generic.List<VehicleImageInfo>(), true, _apiService));
                            return;
                        }
                        else
                        {
                            await ShowSuccessToast("Araç başarıyla eklendi!");
                        }
                    }
                    else
                    {
                        await ShowSuccessToast("Araç başarıyla eklendi!");
                    }

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlertAsync("Hata", message, "Tamam");
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
