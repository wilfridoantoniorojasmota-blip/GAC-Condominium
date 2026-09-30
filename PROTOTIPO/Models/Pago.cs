using System;
using System.Collections.Generic;

namespace PROTOTIPO.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? Monto { get; set; }

    public string? MetodoPago { get; set; }

    public string? EstadoPago { get; set; }

    public int? IdCuota { get; set; }

    public int? IdContrato { get; set; }

    public string TipoPago { get; set; } = null!;

    public virtual ContratoAlquiler? IdContratoNavigation { get; set; }

    public virtual CuotaMantenimiento? IdCuotaNavigation { get; set; }

    public virtual ICollection<TransaccionFinanciera> TransaccionFinancieras { get; set; } = new List<TransaccionFinanciera>();
}
