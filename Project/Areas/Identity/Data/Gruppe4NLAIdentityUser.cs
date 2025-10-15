using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Gruppe4NLA.Areas.Identity.Data;

// Add profile data for application users by adding properties to the Gruppe4NLAIdentityUser class
public abstract class ApplicationUser : IdentityUser
{
    int UserId { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string Organization { get; set; }
    string Email { get; set; }
    string UserName { get; set; }
    string Password { get; set; }
}

public class Pilot : ApplicationUser
{
       bool IsActive { get; set; }
       int PilotId { get; set; }

}

public class  CaseWorkerAdmin : ApplicationUser
{
    bool IsActive { get; set; }
    int CaseWorkerAdminId { get; set; }
}

public class CaseWorker : ApplicationUser
{
    bool IsActive { get; set; }
    int CaseWorkerId { get; set; }
}