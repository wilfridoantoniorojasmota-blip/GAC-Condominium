using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Condominio
{
    public int IdCondominio { get; set; }

    public string? Nombre { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Tamano { get; set; }

    public string? PaginaWeb { get; set; }

    public string? Instagram { get; set; }

    public string? Facebook { get; set; }

    public string? Rnc { get; set; }

    public int? IdSector { get; set; }

    public virtual ICollection<AreaComun> AreaComuns { get; set; } = new List<AreaComun>();

    public virtual ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual Sector? IdSectorNavigation { get; set; }

    public virtual ICollection<TransaccionFinanciera> TransaccionFinancieras { get; set; } = new List<TransaccionFinanciera>();

    public virtual ICollection<UnidadHabitacional> UnidadHabitacionals { get; set; } = new List<UnidadHabitacional>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
