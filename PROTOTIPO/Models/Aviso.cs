using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Aviso
{
    public int IdAviso { get; set; }

    public string? Titulo { get; set; }

    public string? Contenido { get; set; }

    public DateTime? FechaPublicacion { get; set; }

    public int? IdCondominio { get; set; }

    public int? IdUsuario { get; set; }

    public bool Leido { get; set; }

    public string? TipoAviso { get; set; }

    public int? IdReferencia { get; set; }

    public virtual Condominio? IdCondominioNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}