using PROTOTIPO.Models;

namespace PROTOTIPO.Models.ViewModels
{
    public class DashboardTenantVM
    {
        // 👤 Usuario
        public string? NombreInquilino { get; set; }
        public string? Unidad { get; set; }

        // 💰 Finanzas
        public decimal CuotaMensual { get; set; }
        public decimal Balance { get; set; }

        public bool TieneDeuda { get; set; }
        public decimal TotalDeuda { get; set; }

        public DateOnly? ProximoPago { get; set; }

        // 📌 Actividad
        public int IncidenciasActivas { get; set; }

        public List<Aviso> AvisosRecientes { get; set; } = new();
        public List<IncidenciaMantenimiento> IncidenciasRecientes { get; set; } = new();
    }
}