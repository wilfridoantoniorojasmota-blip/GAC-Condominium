using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class ReservaAreaComun
{
    public int IdReserva { get; set; }

    public DateOnly? Fecha { get; set; }

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }

    public string? Estado { get; set; }

    public int? IdArea { get; set; }

    public int? IdUsuario { get; set; }

    public virtual AreaComun? IdAreaNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
