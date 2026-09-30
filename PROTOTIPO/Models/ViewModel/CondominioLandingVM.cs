namespace PROTOTIPO.Models.ViewModel
{
    public class CondominioLandingVM
    {
        public Condominio Condominio { get; set; }

        public List<UnidadLandingVM> Unidades { get; set; }
        public List<UnidadLandingVM> UnidadesDisponibles { get; set; }

        public List<AreaComun> Areas { get; set; }

        public int TotalUnidades { get; set; }

        public int Disponibles { get; set; }

        public int Ocupadas { get; set; }

        public int TotalAreas { get; set; }
    }
}
