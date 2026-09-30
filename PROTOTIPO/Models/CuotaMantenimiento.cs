using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class CuotaMantenimiento
{
    public int IdCuota { get; set; }

    public string? Periodo { get; set; }

    public decimal? Monto { get; set; }

    public string? Estado { get; set; }

    public int? IdUnidad { get; set; }

    public virtual UnidadHabitacional? IdUnidadNavigation { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
