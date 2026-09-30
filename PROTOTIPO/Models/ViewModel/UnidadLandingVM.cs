namespace PROTOTIPO.Models.ViewModel
{
    public class UnidadLandingVM
    {
        public int IdUnidad { get; set; }
        public string Numero { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; }

        public int? CantidadHabitaciones { get; set; }
        public double? CantidadBanos { get; set; }
        public decimal? MetrosCuadrados { get; set; }

        public bool SalaEstar { get; set; }
        public bool Balcon { get; set; }
        public bool Terraza { get; set; }
        public bool Jacuzzi { get; set; }
        public bool Picuzzi { get; set; }
    }
}
