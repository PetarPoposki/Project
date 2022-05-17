using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Project.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ProjectUser class
public class ProjectUser : IdentityUser
{
    public static implicit operator ProjectUser(List<ProjectUser> v)
    {
        throw new NotImplementedException();
    }
   
}

