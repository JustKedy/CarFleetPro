using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CarFleetPro.Mobile.Models
{
    
    
    
    
    public class VehicleAddedMessage : ValueChangedMessage<bool>
    {
        public VehicleAddedMessage() : base(true) { }
    }
}
