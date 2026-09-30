using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Provincium
{
    public int IdProvincia { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Sector> Sectors { get; set; } = new List<Sector>();
}
