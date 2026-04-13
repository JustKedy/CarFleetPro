using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;

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

        
        VehicleNameLabel.Text = $"{vehicle.Marka} {vehicle.Model}";
        VehiclePlateLabel.Text = vehicle.Plaka;
    }

    
    public RentalFormPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        _vehicle = null;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCustomers();
    }

    private async Task LoadCustomers()
    {
        _customers = await _apiService.GetCustomerNamesAsync();
        CustomerPicker.ItemsSource = _customers.Select(c => c.FullName).ToList();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null) await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnRentCompleteClicked(object? sender, EventArgs e)
    {
        if (_vehicle == null)
        {
            await DisplayAlertAsync("Hata", "Araç bilgisi bulunamadı.", "Tamam");
            return;
        }

        
        if (CustomerPicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync("Uyarı", "Lütfen bir müşteri seçin.", "Tamam");
            return;
        }

        var selectedCustomer = _customers[CustomerPicker.SelectedIndex];
        var startDate = StartDatePicker.Date ?? DateTime.Today;
        var endDate = EndDatePicker.Date ?? DateTime.Today.AddDays(1);

        if (endDate <= startDate)
        {
            await DisplayAlertAsync("Uyarı", "Dönüş tarihi, teslim tarihinden sonra olmalıdır.", "Tamam");
            return;
        }

        
        decimal.TryParse(DepositEntry.Text, out var deposit);

        var notes = NotesEditor.Text ?? "";

        (bool success, string message) = await _apiService.CreateRentalAsync(
            selectedCustomer.CustomerId,
            _vehicle.Id,
            startDate,
            endDate,
            deposit,
            notes);

        await DisplayAlertAsync(success ? "Başarılı ✅" : "Hata ❌", message, "Tamam");

        if (success)
        {
            
            await Navigation.PopToRootAsync();
        }
    }
}