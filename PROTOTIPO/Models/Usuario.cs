using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROTOTIPO.Models;

public partial class Usuario
{
   
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string? Nombre { get; set; }
    
    public string? Apellido { get; set; }

    
    public string? Cedula { get; set; }

    public string? TipoCliente { get; set; }

    public string? Pasaporte { get; set; }

    public string? Nacionalidad { get; set; }

    public string? RNC { get; set; }

    public string? NombreEmpresa { get; set; }

    public string? PersonaAlojada { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingrese un email válido")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(4, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Seleccione un rol")]
    public int? IdRol { get; set; }

    public int? IdCondominio { get; set; }

    public virtual ICollection<AsignacionUnidad> AsignacionUnidads { get; set; } = new List<AsignacionUnidad>();

    public virtual ICollection<ContratoAlquiler> ContratoAlquilers { get; set; } = new List<ContratoAlquiler>();

    public virtual Condominio? IdCondominioNavigation { get; set; }

    public virtual Rol? IdRolNavigation { get; set; }

    public virtual ICollection<IncidenciaMantenimiento> IncidenciaMantenimientos { get; set; } = new List<IncidenciaMantenimiento>();

    public virtual ICollection<ReservaAreaComun> ReservaAreaComuns { get; set; } = new List<ReservaAreaComun>();

    public virtual ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
}
