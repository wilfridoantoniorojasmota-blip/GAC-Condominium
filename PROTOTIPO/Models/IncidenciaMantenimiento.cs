using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class IncidenciaMantenimiento
{
    public int IdIncidencia { get; set; }

    public string? Descripcion { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaSolicitud { get; set; }

    public DateTime? FechaCierre { get; set; }

    public int? IdUnidad { get; set; }

    public string? TecnicoAsignado { get; set; }

    public int? IdUsuario { get; set; }

    public virtual UnidadHabitacional? IdUnidadNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<ImagenIncidencium> ImagenIncidencia { get; set; } = new List<ImagenIncidencium>();
}
