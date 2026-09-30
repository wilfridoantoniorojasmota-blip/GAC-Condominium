using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class UnidadHabitacional
{
    public int IdUnidad { get; set; }

    public string? Numero { get; set; }

    public string? Tipo { get; set; }

    public string? Estado { get; set; }

    public string? Descripcion { get; set; }

    public int? IdCondominio { get; set; }
    public int CantidadHabitaciones { get; set; }

    public double? CantidadBanos { get; set; }

    public decimal MetrosCuadrados { get; set; }
    public bool SalaEstar { get; set; }

    public bool Balcon { get; set; }

    public bool Terraza { get; set; }

    public bool Jacuzzi { get; set; }

    public bool Picuzzi { get; set; }

    public virtual ICollection<AsignacionUnidad> AsignacionUnidads { get; set; } = new List<AsignacionUnidad>();

    public virtual ICollection<ContratoAlquiler> ContratoAlquilers { get; set; } = new List<ContratoAlquiler>();

    public virtual ICollection<CuotaMantenimiento> CuotaMantenimientos { get; set; } = new List<CuotaMantenimiento>();

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual Condominio? IdCondominioNavigation { get; set; }

    public virtual ICollection<ImagenUnidad> ImagenUnidads { get; set; } = new List<ImagenUnidad>();

    public virtual ICollection<IncidenciaMantenimiento> IncidenciaMantenimientos { get; set; } = new List<IncidenciaMantenimiento>();

    public virtual ICollection<SolicitudIngreso> SolicitudIngresos { get; set; } = new List<SolicitudIngreso>();
}
