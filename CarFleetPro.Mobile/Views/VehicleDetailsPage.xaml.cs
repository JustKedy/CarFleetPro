using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarFleetPro.Mobile.Views;

public partial class VehicleDetailsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Vehicle _selectedVehicle;

    // Fotoğraf listesi ve index
    private List<VehicleImageInfo> _vehicleImages = new();
    private int _currentImageIndex = 0;
    private bool _isAdmin = false;

    public VehicleDetailsPage(Vehicle selectedVehicle)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _selectedVehicle = selectedVehicle;
        BindingContext = selectedVehicle;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Kullanıcı rolünü kontrol et
        var profile = await _apiService.GetProfileAsync();
        _isAdmin = profile?.Role == "Yönetici";

        // Admin ise fotoğraf yükleme butonunu göster
        UploadButton.IsVisible = _isAdmin;

        await Task.WhenAll(
            LoadVehicleDetails(),
            LoadVehicleImages()
        );
    }

    private async Task LoadVehicleDetails()
    {
        var detail = await _apiService.GetVehicleDetailsAsync(_selectedVehicle.Id);
        if (detail != null)
        {
            StatusBadge.Text = detail.Status;
            StatusBadge.BackgroundColor = detail.Status switch
            {
                "MÜSAİT" => Color.FromArgb("#10B981"),
                "DOLU"    => Color.FromArgb("#EF4444"),
                _         => Color.FromArgb("#F59E0B")
            };

            KmLabel.Text = detail.Mileage.ToString("N0");
            FuelLabel.Text = detail.FuelType;
            GearLabel.Text = detail.TransmissionType;

            if (detail.History.Count > 0)
                HistoryList.ItemsSource = detail.History;
        }
    }

    private async Task LoadVehicleImages()
    {
        _vehicleImages = await _apiService.GetVehicleImagesAsync(_selectedVehicle.Id);

        if (_vehicleImages.Count == 0)
        {
            // Fotoğraf yoksa placeholder göster
            PlaceholderImage.IsVisible = true;
            PrimaryImage.IsVisible = false;
            PhotoCountBadge.IsVisible = false;
            ViewAllButton.IsVisible = false;
            PhotoStripBorder.IsVisible = false;
            return;
        }

        // Birincil fotoğrafı bul ve göster
        var primaryImage = _vehicleImages.FirstOrDefault(i => i.IsPrimary) ?? _vehicleImages[0];
        _currentImageIndex = _vehicleImages.IndexOf(primaryImage);
        ShowImageAtIndex(_currentImageIndex);

        // Fotoğraf sayacını güncelle
        PhotoCountBadge.IsVisible = _vehicleImages.Count > 1;
        PhotoCountLabel.Text = $"{_currentImageIndex + 1}/{_vehicleImages.Count}";

        // Birden fazla fotoğraf varsa "Tüm Fotoğraflar" butonunu göster
        ViewAllButton.IsVisible = _vehicleImages.Count > 1;

        // Küçük önizleme şeridini doldur
        if (_vehicleImages.Count > 1)
        {
            PhotoStrip.ItemsSource = _vehicleImages;
            PhotoStripBorder.IsVisible = true;
        }

        // Kapak fotoğrafı geçişi için swipe gesture ekle
        SetupPhotoSwipe();
    }

    private void ShowImageAtIndex(int index)
    {
        if (index < 0 || index >= _vehicleImages.Count) return;
        _currentImageIndex = index;

        var img = _vehicleImages[index];
        PrimaryImage.Source = ImageSource.FromUri(new Uri(img.ImageUrl));
        PrimaryImage.IsVisible = true;
        PlaceholderImage.IsVisible = false;

        PhotoCountLabel.Text = $"{index + 1}/{_vehicleImages.Count}";
    }

    private void SetupPhotoSwipe()
    {
        // Sol → sağ (önceki)
        var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        swipeLeft.Swiped += (s, e) =>
        {
            var next = (_currentImageIndex + 1) % _vehicleImages.Count;
            ShowImageAtIndex(next);
        };

        // Sağ → sol (sonraki)
        var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipeRight.Swiped += (s, e) =>
        {
            var prev = (_currentImageIndex - 1 + _vehicleImages.Count) % _vehicleImages.Count;
            ShowImageAtIndex(prev);
        };

        PrimaryImage.GestureRecognizers.Clear();
        PrimaryImage.GestureRecognizers.Add(swipeLeft);
        PrimaryImage.GestureRecognizers.Add(swipeRight);
    }

    // ─── Fotoğraf Yükleme (Admin) ───────────────────────────────────────────
    private async void OnUploadPhotoTapped(object? sender, EventArgs e)
    {
        if (!_isAdmin) return;

        // Mevcut fotoğraf sayısı kontrolü
        if (_vehicleImages.Count >= 10)
        {
            await DisplayAlertAsync("Limit", "Bu araç için maksimum 10 fotoğraf yüklenebilir.", "Tamam");
            return;
        }

        var action = await DisplayActionSheetAsync(
            "Fotoğraf Ekle",
            "İptal",
            null,
            "Galeriden Seç",
            "Kamerayı Aç");

        FileResult? photo = null;

        try
        {
            if (action == "Galeriden Seç")
            {
                var results = await MediaPicker.Default.PickPhotosAsync();
                photo = results?.FirstOrDefault();
            }
            else if (action == "Kamerayı Aç" && MediaPicker.Default.IsCaptureSupported)
                photo = await MediaPicker.Default.CapturePhotoAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Hata", $"Fotoğraf seçilemedi: {ex.Message}", "Tamam");
            return;
        }

        if (photo == null) return;

        // Yükleme göstergesi
        UploadButton.IsVisible = false;
        var loadingLabel = new Label
        {
            Text = "⬆️ Yükleniyor...",
            TextColor = Colors.White,
            FontSize = 12,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 15, 15)
        };

        // ContentType belirle
        var contentType = photo.ContentType ?? "image/jpeg";

        var (success, message, newImage) = await _apiService.UploadVehicleImageAsync(
            _selectedVehicle.Id,
            photo.FullPath,
            photo.FileName,
            contentType);

        UploadButton.IsVisible = _isAdmin;

        if (success)
        {
            // Yeni fotoğrafı listeye ekle ve göster
            if (newImage != null)
            {
                _vehicleImages.Add(newImage);

                // Yeni yüklenen fotoğrafa git
                ShowImageAtIndex(_vehicleImages.Count - 1);

                PhotoCountBadge.IsVisible = _vehicleImages.Count > 1;
                ViewAllButton.IsVisible = _vehicleImages.Count > 1;

                if (_vehicleImages.Count > 1)
                {
                    PhotoStrip.ItemsSource = null;
                    PhotoStrip.ItemsSource = _vehicleImages;
                    PhotoStripBorder.IsVisible = true;
                }
            }

            await DisplayAlertAsync("✅ Başarılı", message, "Tamam");
        }
        else
        {
            await DisplayAlertAsync("Hata ❌", message, "Tamam");
        }
    }

    // ─── Tüm Fotoğrafları Gör ─────────────────────────────────────────────
    private async void OnViewAllPhotosTapped(object? sender, EventArgs e)
    {
        if (_vehicleImages.Count == 0) return;
        await Navigation.PushAsync(new VehiclePhotoGalleryPage(_selectedVehicle, _vehicleImages, _isAdmin, _apiService));
    }

    // ─── Standart aksiyonlar ───────────────────────────────────────────────
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null) await Navigation.PopAsync();
    }

    private async void OnMaintenanceClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync("Bakıma Gönder",
            $"{_selectedVehicle.Plaka} plakalı aracı bakıma göndermek istediğinize emin misiniz?",
            "Evet", "İptal");

        if (!confirm) return;

        var (success, message) = await _apiService.SendToMaintenanceAsync(_selectedVehicle.Id);
        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");

        if (success && Navigation is not null)
            await Navigation.PopAsync();
    }

    private async void OnRentClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null)
            await Navigation.PushAsync(new RentalFormPage(_selectedVehicle));
    }
}
