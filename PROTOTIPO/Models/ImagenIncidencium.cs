using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class ImagenIncidencium
{
    public int IdImagen { get; set; }

    public string? Url { get; set; }

    public string? Descripcion { get; set; }

    public int? IdIncidencia { get; set; }

    public virtual IncidenciaMantenimiento? IdIncidenciaNavigation { get; set; }
}
