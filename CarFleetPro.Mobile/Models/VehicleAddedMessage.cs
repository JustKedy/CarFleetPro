using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CarFleetPro.Mobile.Models
{
    /// <summary>
    /// Yeni araç eklendiğinde WeakReferenceMessenger aracılığıyla yayımlanan mesaj.
    /// FleetManagementPage bu mesajı alınca listeyi yeniler.
    /// </summary>
    public class VehicleAddedMessage : ValueChangedMessage<bool>
    {
        public VehicleAddedMessage() : base(true) { }
    }
}
