using System;
using System.Collections.Generic;

namespace AuthApi.Entities;

public partial class Role
{
    public int IdRole { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
