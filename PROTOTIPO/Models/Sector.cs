using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Sector
{
    public int IdSector { get; set; }

    public string? Nombre { get; set; }

    public int? IdProvincia { get; set; }

    public virtual ICollection<Condominio> Condominios { get; set; } = new List<Condominio>();

    public virtual Provincium? IdProvinciaNavigation { get; set; }
}
