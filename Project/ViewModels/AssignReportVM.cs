using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

///<summary>
/// ViewModel for assigning a report to a caseworker.
/// </summary>

namespace Gruppe4NLA.ViewModels 
{
    public class AssignReportVM 
    {
        [Required] public int ReportId { get; set; }
        public string? CurrentAssignee { get; set; }

        [Required(ErrorMessage = "Please select a caseworker")]
        public string? ToUserId { get; set; } // ID of the caseworker to assign the report to

        public List<SelectListItem> Caseworkers { get; set; } = new(); // List of caseworkers for dropdown selection
    }
}