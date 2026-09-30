using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class ImagenUnidad
{
    public int IdImagenUnidad { get; set; }

    public string Url { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdUnidad { get; set; }

    public virtual UnidadHabitacional IdUnidadNavigation { get; set; } = null!;
}
