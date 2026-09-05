using System.Collections.Generic;

namespace DriveFleet.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new List<User>();
}
