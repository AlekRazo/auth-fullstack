using System;
using System.Collections.Generic;

namespace AuthApi.Entities;

public partial class Menu
{
    public int IdMenu { get; set; }

    public string Name { get; set; } = null!;

    public string Link { get; set; } = null!;

    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
