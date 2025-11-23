///<summary> 
/// ViewModel for displaying confirmation messages to users.
/// </summary>

namespace Gruppe4NLA.Models
{
    public class ConfirmationViewModel // Renamed to ConfirmationViewModel for clarity
    {
        public string Title { get; set; } = ""; 
        public string Message { get; set; } = ""; 
        public string RedirectUrl { get; set; } = "";
    }
}
