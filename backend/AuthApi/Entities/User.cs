using System;
using System.Collections.Generic;

namespace AuthApi.Entities;

public partial class User
{
    public int IdUser { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRole { get; set; }

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
