using System;
using System.Collections.Generic;

namespace AuthApi.Entities;

public partial class RefreshToken
{
    public int IdRefreshToken { get; set; }

    public int IdUser { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;
}
