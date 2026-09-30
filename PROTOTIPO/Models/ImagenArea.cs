using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class ImagenArea
{
    public int IdImagen { get; set; }

    public string? Url { get; set; }

    public string? Descripcion { get; set; }

    public int? IdArea { get; set; }

    public virtual AreaComun? IdAreaNavigation { get; set; }
}
