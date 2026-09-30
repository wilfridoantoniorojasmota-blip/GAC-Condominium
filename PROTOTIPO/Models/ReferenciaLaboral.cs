using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROTOTIPO.Models
{
    public class ReferenciaLaboral
    {
        [Key]
        public int IdReferencia { get; set; }

        [Required]
        public int IdSolicitud { get; set; }

        [Required]
        public bool Trabaja { get; set; }

        public string? TipoTrabajo { get; set; }

        public string? Empresa { get; set; }

        public string? Cargo { get; set; }

        public string? TiempoLaborando { get; set; }

        public decimal? IngresoMensual { get; set; }

        public string? TelefonoEmpresa { get; set; }

        public string? DireccionEmpresa { get; set; }


        // Navegación
        [ForeignKey("IdSolicitud")]
        public virtual SolicitudIngreso IdSolicitudNavigation { get; set; }
    }
}