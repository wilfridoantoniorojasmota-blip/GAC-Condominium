using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class AsignacionUnidad
{
    public int IdAsignacion { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Tipo { get; set; }

    public string? Estado { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdUnidad { get; set; }

    public virtual UnidadHabitacional? IdUnidadNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
