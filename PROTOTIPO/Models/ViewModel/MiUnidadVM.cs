namespace PROTOTIPO.Models.ViewModel
{
    public class MiUnidadVM
    {
        public string? NombreInquilino { get; set; }

        public string? NumeroUnidad { get; set; }
        public string? TipoUnidad { get; set; }
        public string? EstadoUnidad { get; set; }
        public string? DescripcionUnidad { get; set; }

        public decimal MontoRenta { get; set; }

        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }

        public string? Email { get; set; }
        public string? Telefono { get; set; }

        public string? ImagenPrincipal { get; set; }
        public decimal CuotaMensual { get; set; }
    }
}
