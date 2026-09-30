namespace PROTOTIPO.Models.ViewModel
{
    public class UnidadDuplicarVM
    {
        public int IdOriginal { get; set; }

        // El signo '?' le dice a ASP.NET Core que permitir nulls es correcto aquí
        public string? Tipo { get; set; }
        public string? Estado { get; set; }
        public string? Descripcion { get; set; }

        public int? IdCondominio { get; set; }

        public int CantidadHabitaciones { get; set; }
        public double? CantidadBanos { get; set; }
        public decimal MetrosCuadrados { get; set; }

        // Este sigue sin '?' porque obligatoriamente el usuario debe escribirlo
        public string Numero { get; set; }

        // La imagen debe ser opcional por si el usuario no quiere subir una nueva foto
        public IFormFile? ImageFile { get; set; }
    }
}