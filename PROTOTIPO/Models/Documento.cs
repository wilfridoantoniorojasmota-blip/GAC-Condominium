using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROTOTIPO.Models;

public partial class Documento
{
    public int IdDocumento { get; set; }

    public string? Nombre { get; set; }

    public string? Tipo { get; set; }

    public string? Archivo { get; set; }

    public DateTime? FechaSubida { get; set; }

    public int? IdCondominio { get; set; }

    public int? IdUnidad { get; set; }
    public int? IdContrato { get; set; }

    public virtual Condominio? IdCondominioNavigation { get; set; }

    public virtual UnidadHabitacional? IdUnidadNavigation { get; set; }
    [ForeignKey("IdContrato")]
    public virtual ContratoAlquiler? IdContratoNavigation { get; set; }
}
