using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

// ViewModel for assigning a report to a caseworker.

namespace Gruppe4NLA.ViewModels 
{
    public class AssignReportVM 
    {
        [Required] public int ReportId { get; set; }
        public string? CurrentAssignee { get; set; }

        [Required(ErrorMessage = "Please select a caseworker")]
        public string? ToUserId { get; set; }

        public List<SelectListItem> Caseworkers { get; set; } = new(); // List of caseworkers for dropdown selection
    }
}