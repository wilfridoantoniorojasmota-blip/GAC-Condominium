namespace PROTOTIPO.Models.ViewModel
{
    public class PagoDTO
    {
        public int IdPago { get; set; }
        public string Residente { get; set; }
        public string? Apellido { get; set; }
        public string Unidad { get; set; }
        public string TipoPago { get; set; }
        public decimal? Monto { get; set; }
        public DateTime? Fecha { get; set; }
        public string Estado { get; set; }
        public int? IdUsuario { get; set; }
    }
}
