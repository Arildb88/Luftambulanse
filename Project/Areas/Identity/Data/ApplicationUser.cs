using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

// Creates a ApplicationUser class thats inherits from IdentityUser, it inherits all properties from IdentityUser
// and adds a new property for users "Organization"

namespace Gruppe4NLA.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
        public string? Organization { get; set; }
}

