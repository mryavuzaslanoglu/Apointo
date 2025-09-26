namespace Apointo.Domain.Identity;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Customer = "Customer";

    public static readonly IReadOnlyCollection<string> All = new[] { Admin, Staff, Customer };
}