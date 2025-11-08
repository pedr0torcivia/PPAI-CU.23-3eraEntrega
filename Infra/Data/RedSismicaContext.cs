using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using E = PPAI_2.Infra.Data.EFModels; // Entidades de Infraestructura
using M = PPAI_Revisiones.Modelos; // Entidades de Dominio (No usadas directamente aquí, pero sí sus configuraciones)
using Microsoft.EntityFrameworkCore.InMemory;

namespace PPAI_2.Infra.Data
{
    public class RedSismicaContext : DbContext
    {
        public DbSet<E.EmpleadoEF> Empleados { get; set; }
        public DbSet<E.UsuarioEF> Usuarios { get; set; }
        public DbSet<E.EventoSismicoEF> EventosSismicos { get; set; }
        public DbSet<E.CambioDeEstadoEF> CambiosDeEstado { get; set; }
        public DbSet<E.EstacionSismologicaEF> EstacionesSismologicas { get; set; }
        public DbSet<E.SismografoEF> Sismografos { get; set; }
        public DbSet<E.SerieTemporalEF> SeriesTemporales { get; set; }
        public DbSet<E.MuestraSismicaEF> MuestrasSismicas { get; set; }
        public DbSet<E.DetalleMuestraSismicaEF> DetallesMuestrasSismicas { get; set; }
        public DbSet<E.TipoDeDatoEF> TiposDeDatos { get; set; }

        public RedSismicaContext(DbContextOptions<RedSismicaContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // --- Empleado ---
            modelBuilder.Entity<E.EmpleadoEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("EmpleadoId");
                entity.HasKey("Id");
                entity.Property(e => e.Rol).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            });

            // --- Usuario ---
            modelBuilder.Entity<E.UsuarioEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("UsuarioId");
                entity.HasKey("Id");
                entity.Property<Guid>("EmpleadoId");
                entity.HasOne(u => u.Empleado)
                      .WithOne()
                      .HasForeignKey<E.UsuarioEF>("EmpleadoId")
                      .IsRequired();
            });

            // --- EventoSismico ---
            modelBuilder.Entity<E.EventoSismicoEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("EventoSismicoId");
                entity.HasKey("Id");
                entity.HasOne(e => e.Clasificacion).WithMany().IsRequired();
                entity.HasOne(e => e.Alcance).WithMany().IsRequired();
                entity.HasOne(e => e.Origen).WithMany().IsRequired();
                entity.Property(e => e.EstadoActualNombre).IsRequired().HasMaxLength(50);
            });

            // --- CambioDeEstado ---
            modelBuilder.Entity<E.CambioDeEstadoEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("CambioDeEstadoId");
                entity.HasKey("Id");

                // CORRECCIÓN: Usamos el nombre de propiedad de navegación real: .Evento (asumo que es ese)
                entity.Property<Guid>("EventoSismicoId");
                entity.HasOne(ce => ce.Evento) // <--- CORRECCIÓN (Era .EventoSismico)
                      .WithMany(ev => ev.CambiosDeEstado)
                      .HasForeignKey("EventoSismicoId");

                entity.Property<Guid?>("ResponsableId");
                entity.HasOne(ce => ce.Responsable)
                      .WithMany()
                      .HasForeignKey("ResponsableId")
                      .IsRequired(false);
            });

            // --- EstacionSismologica ---
            modelBuilder.Entity<E.EstacionSismologicaEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("EstacionSismologicaId");
                entity.HasKey("Id");
                entity.HasIndex(e => e.CodigoEstacion).IsUnique();
            });

            // --- Sismografo ---
            modelBuilder.Entity<E.SismografoEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("SismografoId");
                entity.HasKey("Id");
                entity.Property<Guid>("EstacionId");
                entity.HasOne(s => s.Estacion)
                      .WithMany(es => es.Sismografos)
                      .HasForeignKey("EstacionId");
            });

            // --- SerieTemporal ---
            modelBuilder.Entity<E.SerieTemporalEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("SerieTemporalId");
                entity.HasKey("Id");

                // CORRECCIÓN: Usamos el nombre de propiedad de navegación real: .Evento
                entity.Property<Guid>("EventoSismicoId");
                entity.HasOne(s => s.Evento) // <--- CORRECCIÓN (Era .EventoSismico)
                      .WithMany(e => e.SeriesTemporales)
                      .HasForeignKey("EventoSismicoId");
            });

            // --- MuestraSismica ---
            modelBuilder.Entity<E.MuestraSismicaEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("MuestraSismicaId");
                entity.HasKey("Id");

                // CORRECCIÓN: Usamos el nombre de propiedad de navegación real: .Serie
                entity.Property<Guid>("SerieTemporalId");
                entity.HasOne(m => m.Serie) // <--- CORRECCIÓN (Era .SerieTemporal)
                      .WithMany(s => s.Muestras)
                      .HasForeignKey("SerieTemporalId");
            });

            // --- DetalleMuestraSismica ---
            modelBuilder.Entity<E.DetalleMuestraSismicaEF>(entity =>
            {
                entity.Property<Guid>("Id").HasColumnName("DetalleMuestraSismicaId");
                entity.HasKey("Id");

                // CORRECCIÓN: Usamos el nombre de propiedad de navegación real: .Muestra
                entity.Property<Guid>("MuestraSismicaId");
                entity.HasOne(d => d.Muestra) // <--- CORRECCIÓN (Era .MuestraSismica)
                      .WithMany(m => m.Detalles)
                      .HasForeignKey("MuestraSismicaId");

                entity.HasOne(d => d.TipoDeDato).WithMany().IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}