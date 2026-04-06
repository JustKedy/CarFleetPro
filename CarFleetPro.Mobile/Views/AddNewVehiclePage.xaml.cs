using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Models; // Araç modeli için gerekli

namespace CarFleetPro.Mobile.Views
{
    public partial class AddNewVehiclePage : ContentPage
    {
        private Vehicle _duzenlenenArac; // Hangi aracı düzenlediğimizi aklımızda tutuyoruz

        // 1. DURUM: "Yeni Araç Ekle" Butonuna Basıldığında Burası Çalışır (Bomboş Form)
        public AddNewVehiclePage()
        {
            InitializeComponent();
        }

        // 2. DURUM: "Düzenle" (Kalem İkonu) Basıldığında Burası Çalışır (Dolu Form)
        public AddNewVehiclePage(Vehicle secilenArac)
        {
            InitializeComponent();
            _duzenlenenArac = secilenArac;

            // Arayüzü düzenleme moduna geçir
            SayfayiDuzenlemeModunaGecir();
        }

        private void SayfayiDuzenlemeModunaGecir()
        {
            // Sayfa başlığını değiştir
            PageTitleLabel.Text = "ARAÇ DÜZENLE";

            // Kutuları (Entry/Picker) seçilen aracın bilgileriyle doldur
            PlakaEntry.Text = _duzenlenenArac.Plaka;
            KmEntry.Text = _duzenlenenArac.Km.ToString();
            HpEntry.Text = _duzenlenenArac.Hp.ToString();

            // Yaştan üretim yılını buluyoruz (Mevcut Yıl - Yaş)
            YilEntry.Text = (DateTime.Now.Year - _duzenlenenArac.Yas).ToString();

            // Durum Picker'ı ayarla
            DurumPicker.SelectedItem = _duzenlenenArac.Durum;

            // Not: Marka ve Model API'den (Master Data) gelince Picker'lar dolacak.
            // Onları şimdilik bırakıyoruz.
        }

        public async void OnUploadImageTapped(object sender, EventArgs e)
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult photo = await MediaPicker.Default.PickPhotoAsync();
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

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_duzenlenenArac != null)
            {
                // TODO: Güncelleme (PUT) API'si buraya yazılacak
                await ShowSuccessToast("Araç başarıyla güncellendi kiral!");
            }
            else
            {
                // TODO: Yeni Ekleme (POST) API'si buraya yazılacak
                await ShowSuccessToast("Araç başarıyla eklendi kiral!");
            }

            await Navigation.PopAsync();
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
                Content = new Label
                {
                    Text = message,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                }
            };

            var mainLayout = this.Content as Grid;
            if (mainLayout != null)
            {
                Grid.SetRowSpan(toastGrid, mainLayout.RowDefinitions.Count > 0 ? mainLayout.RowDefinitions.Count : 1);
                mainLayout.Children.Add(toastGrid);
                toastGrid.Scale = 0.8;
                await Task.WhenAll(toastGrid.FadeTo(1, 250, Easing.CubicOut), toastGrid.ScaleTo(1, 250, Easing.CubicOut));
                await Task.Delay(2000);
                await toastGrid.FadeTo(0, 300, Easing.CubicIn);
                mainLayout.Children.Remove(toastGrid);
            }
        }
    }
}