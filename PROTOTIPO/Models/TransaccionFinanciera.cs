using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class TransaccionFinanciera
{
    public int IdTransaccion { get; set; }

    public string? Tipo { get; set; }

    public string? Descripcion { get; set; }

    public decimal? Monto { get; set; }

    public DateTime? Fecha { get; set; }

    public int? IdCondominio { get; set; }

    public int? IdPago { get; set; }

    public virtual Condominio? IdCondominioNavigation { get; set; }

    public virtual Pago? IdPagoNavigation { get; set; }
}
