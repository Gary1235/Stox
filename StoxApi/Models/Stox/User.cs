using System;
using System.Collections.Generic;

namespace StoxApi.Models.Stox;

public partial class User
{
    public Guid Id { get; set; }

    public string Account { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
