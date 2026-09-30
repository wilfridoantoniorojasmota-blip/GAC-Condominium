using PROTOTIPO.Models.ViewModel;

namespace PROTOTIPO.Models.ViewModel
{
    public class DashboardVM
    {
        public int TotalUnidades { get; set; }
        public int ResidentesActivos { get; set; }
        public int CuotasPendientes { get; set; }
        public decimal IngresosMes { get; set; }

        public List<ActividadDTO> ActividadesRecientes { get; set; }
        public EstadoUnidadesDTO EstadoUnidades { get; set; }

        public List<PagoDTO> PagosRecientes { get; set; }

        // Ingresos de los últimos 6 meses para el gráfico
        public List<decimal> IngresosUltimos6Meses { get; set; } = new();
    }
}
