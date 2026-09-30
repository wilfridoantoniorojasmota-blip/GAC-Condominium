using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class ContratoAlquiler
{
    public int IdContrato { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public decimal? MontoRenta { get; set; }

    public string? Estado { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdUnidad { get; set; }

    public virtual UnidadHabitacional? IdUnidadNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public virtual ICollection<Documento> Documentos { get; set; }
    = new List<Documento>();
}
