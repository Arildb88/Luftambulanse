// ViewModel for error handling in an ASP.NET Core MVC application

namespace Gruppe4NLA.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
