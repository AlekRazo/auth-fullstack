using System;
using System.Collections.Generic;

namespace AuthApi.Entities;

public partial class RoleMenu
{
    public int IdRoleMenu { get; set; }

    public int IdRole { get; set; }

    public int IdMenu { get; set; }

    public virtual Menu IdMenuNavigation { get; set; } = null!;

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
