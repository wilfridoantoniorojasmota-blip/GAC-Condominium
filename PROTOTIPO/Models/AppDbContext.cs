using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PROTOTIPO.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AreaComun> AreaComuns { get; set; }

    public virtual DbSet<AsignacionUnidad> AsignacionUnidads { get; set; }

    public virtual DbSet<Aviso> Avisos { get; set; }

    public virtual DbSet<Condominio> Condominios { get; set; }

    public virtual DbSet<ContratoAlquiler> ContratoAlquilers { get; set; }

    public virtual DbSet<CuotaMantenimiento> CuotaMantenimientos { get; set; }

    public virtual DbSet<Documento> Documentos { get; set; }

    public virtual DbSet<ImagenArea> ImagenAreas { get; set; }

    public virtual DbSet<ImagenIncidencium> ImagenIncidencia { get; set; }

    public virtual DbSet<ImagenUnidad> ImagenUnidads { get; set; }

    public virtual DbSet<IncidenciaMantenimiento> IncidenciaMantenimientos { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Provincium> Provincia { get; set; }

    public virtual DbSet<ReservaAreaComun> ReservaAreaComuns { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Sector> Sectors { get; set; }

    public virtual DbSet<SolicitudIngreso> SolicitudIngresos { get; set; }
    public virtual DbSet<ReferenciaLaboral> ReferenciaLaborals { get; set; }
    
    public virtual DbSet<TransaccionFinanciera> TransaccionFinancieras { get; set; }

    public virtual DbSet<UnidadHabitacional> UnidadHabitacionals { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ROJAS;Database=DB_Condominios;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AreaComun>(entity =>
        {
            entity.HasKey(e => e.IdArea).HasName("PK__AreaComu__8A8C837BF9FAD36A");

            entity.ToTable("AreaComun");

            entity.Property(e => e.IdArea).HasColumnName("id_area");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.AreaComuns)
                .HasForeignKey(d => d.IdCondominio)
                .HasConstraintName("FK__AreaComun__id_co__5EBF139D");
        });

        modelBuilder.Entity<AsignacionUnidad>(entity =>
        {
            entity.HasKey(e => e.IdAsignacion).HasName("PK__Asignaci__C3F7F9666F86ABAE");

            entity.ToTable("AsignacionUnidad");

            entity.Property(e => e.IdAsignacion).HasColumnName("id_asignacion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdUnidadNavigation).WithMany(p => p.AsignacionUnidads)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK__Asignacio__id_un__5812160E");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AsignacionUnidads)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Asignacio__id_us__571DF1D5");
        });

        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.HasKey(e => e.IdAviso).HasName("PK__Aviso__66D8457B0BEE9808");

            entity.ToTable("Aviso");

            entity.Property(e => e.IdAviso).HasColumnName("id_aviso");

            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("titulo");

            entity.Property(e => e.Contenido)
                .HasColumnType("text")
                .HasColumnName("contenido");

            entity.Property(e => e.FechaPublicacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_publicacion");

            entity.Property(e => e.IdCondominio)
                .HasColumnName("id_condominio");

            // ===== NUEVOS CAMPOS =====

            entity.Property(e => e.IdUsuario)
                .HasColumnName("IdUsuario");

            entity.Property(e => e.Leido)
                .HasDefaultValue(false);

            entity.Property(e => e.TipoAviso)
                .HasMaxLength(50);

            entity.Property(e => e.IdReferencia);

            // =========================

            entity.HasOne(d => d.IdCondominioNavigation)
                .WithMany(p => p.Avisos)
                .HasForeignKey(d => d.IdCondominio)
                .HasConstraintName("FK__Aviso__id_condom__4D94879B");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Avisos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Aviso_Usuario");
        });

        modelBuilder.Entity<Condominio>(entity =>
        {
            entity.HasKey(e => e.IdCondominio).HasName("PK__Condomin__2D444BD1B6A72F17");

            entity.ToTable("Condominio");

            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.Direccion)
                .HasColumnType("text")
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Facebook)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("facebook");
            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.Instagram)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("instagram");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PaginaWeb)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("pagina_web");
            entity.Property(e => e.Rnc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("rnc");
            entity.Property(e => e.Tamano)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tamano");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdSectorNavigation).WithMany(p => p.Condominios)
                .HasForeignKey(d => d.IdSector)
                .HasConstraintName("FK__Condomini__id_se__3C69FB99");
        });

        modelBuilder.Entity<ContratoAlquiler>(entity =>
        {
            entity.HasKey(e => e.IdContrato).HasName("PK__Contrato__FF5F2A569A5671BF");

            entity.ToTable("ContratoAlquiler");

            entity.Property(e => e.IdContrato).HasColumnName("id_contrato");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MontoRenta)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_renta");

            entity.HasOne(d => d.IdUnidadNavigation).WithMany(p => p.ContratoAlquilers)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK__ContratoA__id_un__5BE2A6F2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ContratoAlquilers)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__ContratoA__id_us__5AEE82B9");
        });

        modelBuilder.Entity<CuotaMantenimiento>(entity =>
        {
            entity.HasKey(e => e.IdCuota).HasName("PK__CuotaMan__5EFF6F7E419A6EB4");

            entity.ToTable("CuotaMantenimiento");

            entity.Property(e => e.IdCuota).HasColumnName("id_cuota");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Periodo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("periodo");

            entity.HasOne(d => d.IdUnidadNavigation).WithMany(p => p.CuotaMantenimientos)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK__CuotaMant__id_un__6EF57B66");
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(e => e.IdDocumento)
                .HasName("PK__Document__5D2EE7E50A8829A9");

            entity.ToTable("Documento");

            entity.Property(e => e.IdDocumento)
                .HasColumnName("id_documento");

            entity.Property(e => e.Archivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("archivo");

            entity.Property(e => e.FechaSubida)
                .HasColumnType("datetime")
                .HasColumnName("fecha_subida");

            entity.Property(e => e.IdCondominio)
                .HasColumnName("id_condominio");

            entity.Property(e => e.IdUnidad)
                .HasColumnName("id_unidad");

            entity.Property(e => e.IdContrato)
                .HasColumnName("IdContrato");

            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");


            entity.HasOne(d => d.IdCondominioNavigation)
                .WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdCondominio)
                .HasConstraintName("FK__Documento__id_co__5070F446");


            entity.HasOne(d => d.IdUnidadNavigation)
                .WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK__Documento__id_un__5165187F");


            // NUEVA RELACIÓN CONTRATO
            entity.HasOne(d => d.IdContratoNavigation)
                .WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdContrato)
                .HasConstraintName("FK_Documento_ContratoAlquiler");

        });

        modelBuilder.Entity<ImagenArea>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__ImagenAr__27CC26892DA86519");

            entity.ToTable("ImagenArea");

            entity.Property(e => e.IdImagen).HasColumnName("id_imagen");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.IdArea).HasColumnName("id_area");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("url");

            entity.HasOne(d => d.IdAreaNavigation).WithMany(p => p.ImagenAreas)
                .HasForeignKey(d => d.IdArea)
                .HasConstraintName("FK__ImagenAre__id_ar__656C112C");
        });

        modelBuilder.Entity<ImagenIncidencium>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__ImagenIn__27CC26893CE57700");

            entity.Property(e => e.IdImagen).HasColumnName("id_imagen");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.IdIncidencia).HasColumnName("id_incidencia");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("url");

            entity.HasOne(d => d.IdIncidenciaNavigation).WithMany(p => p.ImagenIncidencia)
                .HasForeignKey(d => d.IdIncidencia)
                .HasConstraintName("FK__ImagenInc__id_in__6C190EBB");
        });

        modelBuilder.Entity<ImagenUnidad>(entity =>
        {
            entity.HasKey(e => e.IdImagenUnidad).HasName("PK__ImagenUn__5F649A347C1DAAD6");

            entity.ToTable("ImagenUnidad");

            entity.HasIndex(e => e.IdUnidad, "IX_ImagenUnidad_Unidad");

            entity.Property(e => e.IdImagenUnidad).HasColumnName("id_imagen_unidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.Url)
                .HasMaxLength(500)
                .HasColumnName("url");

            entity.HasOne(d => d.IdUnidadNavigation).WithMany(p => p.ImagenUnidads)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK_ImagenUnidad_Unidad");
        });

        modelBuilder.Entity<IncidenciaMantenimiento>(entity =>
        {
            entity.HasKey(e => e.IdIncidencia).HasName("PK__Incidenc__D7757E76EBA53F5B");

            entity.ToTable("IncidenciaMantenimiento");

            entity.Property(e => e.IdIncidencia).HasColumnName("id_incidencia");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaCierre)
                .HasColumnType("datetime")
                .HasColumnName("fecha_cierre");
            entity.Property(e => e.FechaSolicitud)
                .HasColumnType("datetime")
                .HasColumnName("fecha_solicitud");
            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdUnidadNavigation).WithMany(p => p.IncidenciaMantenimientos)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK__Incidenci__id_un__68487DD7");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.IncidenciaMantenimientos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Incidenci__id_us__693CA210");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__Pago__0941B07429B0C91C");

            entity.ToTable("Pago");

            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado_pago");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdContrato).HasColumnName("id_contrato");
            entity.Property(e => e.IdCuota).HasColumnName("id_cuota");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.TipoPago)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Otro");

            entity.HasOne(d => d.IdContratoNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdContrato)
                .HasConstraintName("FK__Pago__id_contrat__72C60C4A");

            entity.HasOne(d => d.IdCuotaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdCuota)
                .HasConstraintName("FK__Pago__id_cuota__71D1E811");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PK__Permiso__228F224FAB483F5A");

            entity.ToTable("Permiso");

            entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Provincium>(entity =>
        {
            entity.HasKey(e => e.IdProvincia).HasName("PK__Provinci__66C18BFDF0DE3D23");

            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<ReservaAreaComun>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__ReservaA__423CBE5DEA523E29");

            entity.ToTable("ReservaAreaComun");

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.IdArea).HasColumnName("id_area");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdAreaNavigation).WithMany(p => p.ReservaAreaComuns)
                .HasForeignKey(d => d.IdArea)
                .HasConstraintName("FK__ReservaAr__id_ar__619B8048");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ReservaAreaComuns)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__ReservaAr__id_us__628FA481");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__6ABCB5E0CDDCB23B");

            entity.ToTable("Rol");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasMany(d => d.IdPermisos).WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("IdPermiso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__RolPermis__id_pe__46E78A0C"),
                    l => l.HasOne<Rol>().WithMany()
                        .HasForeignKey("IdRol")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__RolPermis__id_ro__45F365D3"),
                    j =>
                    {
                        j.HasKey("IdRol", "IdPermiso").HasName("PK__RolPermi__889447C4BB5D04CA");
                        j.ToTable("RolPermiso");
                        j.IndexerProperty<int>("IdRol").HasColumnName("id_rol");
                        j.IndexerProperty<int>("IdPermiso").HasColumnName("id_permiso");
                    });
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.HasKey(e => e.IdSector).HasName("PK__Sector__3483C369344647D3");

            entity.ToTable("Sector");

            entity.Property(e => e.IdSector).HasColumnName("id_sector");
            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Sectors)
                .HasForeignKey(d => d.IdProvincia)
                .HasConstraintName("FK__Sector__id_provi__398D8EEE");
        });

        modelBuilder.Entity<SolicitudIngreso>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PK__Solicitu__5C0C31F3D602C1BB");

            entity.ToTable("SolicitudIngreso");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Contacto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("contacto");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.NombreSolicitante)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre_solicitante");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoSolicitud)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_solicitud");

            entity.HasOne(d => d.IdUnidadNavigation).WithMany(p => p.SolicitudIngresos)
                .HasForeignKey(d => d.IdUnidad)
                .HasConstraintName("FK__Solicitud__id_un__5441852A");

            entity.Property(e => e.TipoCliente)
    .HasMaxLength(20)
    .IsUnicode(false)
    .HasColumnName("TipoCliente");

            entity.Property(e => e.Pasaporte)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Pasaporte");

            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("Nacionalidad");

            entity.Property(e => e.RNC)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RNC");

            entity.Property(e => e.NombreEmpresa)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("NombreEmpresa");

            entity.Property(e => e.PersonaAlojada)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("PersonaAlojada");
        });
        modelBuilder.Entity<ReferenciaLaboral>(entity =>
        {
            entity.HasKey(e => e.IdReferencia);

            entity.ToTable("ReferenciaLaboral");

            entity.Property(e => e.TipoTrabajo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Empresa)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.Property(e => e.Cargo)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.TiempoLaborando)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.IngresoMensual)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TelefonoEmpresa)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.DireccionEmpresa)
                .HasMaxLength(200)
                .IsUnicode(false);


            entity.HasOne(d => d.IdSolicitudNavigation)
                .WithOne(p => p.ReferenciaLaboral)
                .HasForeignKey<ReferenciaLaboral>(d => d.IdSolicitud)
                .HasConstraintName("FK_ReferenciaLaboral_SolicitudIngreso");
        });

        modelBuilder.Entity<TransaccionFinanciera>(entity =>
        {
            entity.HasKey(e => e.IdTransaccion).HasName("PK__Transacc__1EDAC099EE2EC7A6");

            entity.ToTable("TransaccionFinanciera");

            entity.Property(e => e.IdTransaccion).HasColumnName("id_transaccion");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.TransaccionFinancieras)
                .HasForeignKey(d => d.IdCondominio)
                .HasConstraintName("FK__Transacci__id_co__75A278F5");

            entity.HasOne(d => d.IdPagoNavigation).WithMany(p => p.TransaccionFinancieras)
                .HasForeignKey(d => d.IdPago)
                .HasConstraintName("FK__Transacci__id_pa__76969D2E");
        });

        modelBuilder.Entity<UnidadHabitacional>(entity =>
        {
            entity.HasKey(e => e.IdUnidad).HasName("PK__UnidadHa__95D7C92B67DC01E1");

            entity.ToTable("UnidadHabitacional");

            entity.Property(e => e.IdUnidad).HasColumnName("id_unidad");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("numero");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.UnidadHabitacionals)
                .HasForeignKey(d => d.IdCondominio)
                .HasConstraintName("FK__UnidadHab__id_co__3F466844");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__4E3E04ADFC0DE764");

            entity.ToTable("Usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.IdCondominio).HasColumnName("id_condominio");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdCondominioNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdCondominio)
                .HasConstraintName("FK__Usuario__id_cond__4AB81AF0");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Usuario__id_rol__49C3F6B7");
            entity.Property(e => e.TipoCliente)
    .HasMaxLength(20)
    .IsUnicode(false)
    .HasColumnName("TipoCliente");

            entity.Property(e => e.Pasaporte)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Pasaporte");

            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("Nacionalidad");

            entity.Property(e => e.RNC)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RNC");

            entity.Property(e => e.NombreEmpresa)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("NombreEmpresa");

            entity.Property(e => e.PersonaAlojada)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("PersonaAlojada");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
