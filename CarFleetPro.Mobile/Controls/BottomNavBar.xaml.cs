using Microsoft.Maui.Controls;
using System;
using CarFleetPro.Mobile.Views; // Sayfalara (HomePage, GaragePage vb.) ulaşmak için ekledik

namespace CarFleetPro.Mobile.Controls;

public partial class BottomNavBar : ContentView
{
    // Dışarıdan "Home", "Garage", "List" gibi parametreler almamızı sağlayan özellik
    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(nameof(SelectedTab), typeof(string), typeof(BottomNavBar), "Home", propertyChanged: OnSelectedTabChanged);

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public BottomNavBar()
    {
        InitializeComponent();
        UpdateUI(SelectedTab); // İlk açılışta UI'ı güncelle
    }

    private static void OnSelectedTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BottomNavBar)bindable;
        control.UpdateUI((string)newValue);
    }

    // Seçilen sekmeye göre ikonları pozitif/negatif yapan motor
    private void UpdateUI(string activeTab)
    {
        // 1. Önce hepsini pasif (negatif) ve çerçevesiz yap
        HomeBorder.Stroke = Colors.Transparent;
        HomeImg.Source = "homenegatif.svg";

        GarageBorder.Stroke = Colors.Transparent;
        GarageImg.Source = "garagenegatif.svg";

        ListBorder.Stroke = Colors.Transparent;
        ListImg.Source = "listnegatif.svg";

        SettingsBorder.Stroke = Colors.Transparent;
        SettingsImg.Source = "settingsnegatif.svg";

        // 2. Sadece seçili olanı aktif (pozitif) ve mavi çerçeveli yap
        var activeColor = Color.FromArgb("#3B82F6");

        switch (activeTab)
        {
            case "Home":
                HomeBorder.Stroke = activeColor;
                HomeImg.Source = "homepozitif.svg";
                break;
            case "Garage":
                GarageBorder.Stroke = activeColor;
                GarageImg.Source = "garagepozitif.svg";
                break;
            case "List":
                ListBorder.Stroke = activeColor;
                ListImg.Source = "listpozitif.svg";
                break;
            case "Settings":
                SettingsBorder.Stroke = activeColor;
                SettingsImg.Source = "settingspozitif.svg";
                break;
        }
    }

    // ----- YENİ EKLENEN: SAYFA GEÇİŞ (NAVİGASYON) METOTLARI -----

    private async void OnHomeTapped(object? sender, TappedEventArgs e)
    {
        // Zaten bu sayfadaysak hiçbir şey yapma
        if (SelectedTab == "Home") return;

        // false parametresi animasyonu kapatır, alt menü sekmesi gibi anında geçer
        await Navigation.PushAsync(new HomePage(), false);
    }

    private async void OnGarageTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "Garage") return;
        await Navigation.PushAsync(new GaragePage(), false);
    }

    private async void OnListTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "List") return;
        await Navigation.PushAsync(new FleetManagementPage(), false);
    }

    private async void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "Settings") return;
        await Navigation.PushAsync(new SettingsPage(), false);
    }
}