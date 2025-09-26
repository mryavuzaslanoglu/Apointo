using System;
using Microsoft.AspNetCore.Identity;

namespace Apointo.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
}