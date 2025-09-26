using System;
using Microsoft.AspNetCore.Identity;

namespace Apointo.Infrastructure.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}