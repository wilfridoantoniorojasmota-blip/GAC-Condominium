using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Rol
{
    public int IdRol { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Permiso> IdPermisos { get; set; } = new List<Permiso>();
}
