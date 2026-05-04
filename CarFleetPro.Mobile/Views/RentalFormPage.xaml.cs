using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace CarFleetPro.Mobile.Views;

public partial class RentalFormPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly Vehicle? _vehicle;
    private List<CustomerName> _customers = new();

    
    public RentalFormPage(Vehicle vehicle)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _vehicle = vehicle;

        VehicleNameLabel.Text  = $"{vehicle.Marka} {vehicle.Model}";
        VehiclePlateLabel.Text = vehicle.Plaka;
    }

    
    public RentalFormPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa açılırken yumuşak bir geçiş efekti
        this.Opacity = 0;
        await this.FadeToAsync(1, 400, Easing.CubicOut);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCompleteRentalClicked(object? sender, EventArgs e)
    {
        // ####### YUNUS İÇİN NOT #######
        // Kanka buraya kiralama kaydını veritabanına atacak POST isteği gelecek.
        // Formdaki verileri toplayıp ApiService.CreateRentalAsync(request) demelisin.
        // Kayıt başarılıysa kullanıcıyı Dashboard'a veya Garaj'a geri yönlendireceğiz.
        // ##############################

        await DisplayAlertAsync("Bilgi", "Kiralama işlemi başarılı bir şekilde kaydedildi.", "Tamam");
        await Navigation.PopAsync();
    }
}