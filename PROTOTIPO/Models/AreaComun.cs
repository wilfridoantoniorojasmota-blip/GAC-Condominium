using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class AreaComun
{
    public int IdArea { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public int? IdCondominio { get; set; }

    public virtual Condominio? IdCondominioNavigation { get; set; }

    public virtual ICollection<ImagenArea> ImagenAreas { get; set; } = new List<ImagenArea>();

    public virtual ICollection<ReservaAreaComun> ReservaAreaComuns { get; set; } = new List<ReservaAreaComun>();
}
