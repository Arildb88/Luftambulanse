/// <summary>
/// ViewModel for error handling in an ASP.NET Core MVC application
/// </summary>

namespace Gruppe4NLA.Models
{
    // ViewModel for error handling
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
