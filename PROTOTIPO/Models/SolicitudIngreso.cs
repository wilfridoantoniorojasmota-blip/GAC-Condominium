using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class SolicitudIngreso
{
    public int IdSolicitud { get; set; }

    public string? NombreSolicitante { get; set; }
    public string? Apellido { get; set; }

    public string? Contacto { get; set; }
    public string? Cedula { get; set; }

    public string? Telefono { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Estado { get; set; }

    public string? TipoSolicitud { get; set; }

    public string? Observaciones { get; set; }

    public int? IdUnidad { get; set; }

    public virtual UnidadHabitacional? IdUnidadNavigation { get; set; }
    public virtual ReferenciaLaboral? ReferenciaLaboral { get; set; }

    public string? TipoCliente { get; set; }

    public string? Pasaporte { get; set; }

    public string? Nacionalidad { get; set; }

    public string? RNC { get; set; }

    public string? NombreEmpresa { get; set; }

    public string? PersonaAlojada { get; set; }




}
