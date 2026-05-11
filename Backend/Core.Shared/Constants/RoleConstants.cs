using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Shared.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";

    public static readonly string[] AllRoles = { Admin, Staff };

    public static bool IsValid(string? role) =>
        AllRoles.Contains(role);
}

