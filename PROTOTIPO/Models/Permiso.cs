using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Permiso
{
    public int IdPermiso { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
