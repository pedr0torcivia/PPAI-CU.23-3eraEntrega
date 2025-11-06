// RedSismicaContext.cs
using Microsoft.EntityFrameworkCore;
using PPAI_Revisiones.Modelos;
using System;

namespace PPAI_2.Infra.Data
{
    public class RedSismicaContext : DbContext
    {
        public DbSet<EventoSismico> EventosSismicos => Set<EventoSismico>();
        public DbSet<CambioDeEstado> CambiosDeEstado => Set<CambioDeEstado>();
        public DbSet<AlcanceSismo> Alcances => Set<AlcanceSismo>();
        public DbSet<ClasificacionSismo> Clasificaciones => Set<ClasificacionSismo>();
        public DbSet<OrigenDeGeneracion> Origenes => Set<OrigenDeGeneracion>();
        public DbSet<SerieTemporal> SeriesTemporales => Set<SerieTemporal>();
        public DbSet<MuestraSismica> Muestras => Set<MuestraSismica>();
        public DbSet<DetalleMuestra> DetallesMuestra => Set<DetalleMuestra>();
        public DbSet<TipoDeDato> TiposDeDato => Set<TipoDeDato>();
        public DbSet<Sismografo> Sismografos => Set<Sismografo>();
        public DbSet<EstacionSismologica> Estaciones => Set<EstacionSismologica>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Empleado> Empleados => Set<Empleado>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redsismica.db");
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ===== EventoSismico =====
            mb.Entity<EventoSismico>(e =>
            {
                e.ToTable("EventosSismicos");
                e.HasKey(x => x.Id);

                e.Ignore(x => x.EstadoActual);    // Solo persisto el nombre
                e.Property(x => x.EstadoActualNombre).HasMaxLength(100);

                e.HasMany(x => x.CambiosDeEstado)
                    .WithOne(c => c.Evento)
                    .HasForeignKey(c => c.EventoSismicoId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(x => x.SeriesTemporales)
                    .WithOne()
                    .HasForeignKey(s => s.EventoSismicoId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FKs explícitas
                e.HasOne(x => x.Alcance).WithMany().HasForeignKey(x => x.AlcanceId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Clasificacion).WithMany().HasForeignKey(x => x.ClasificacionId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Origen).WithMany().HasForeignKey(x => x.OrigenId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Responsable).WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.FechaHoraInicio);
            });

            // ===== CambioDeEstado =====
            mb.Entity<CambioDeEstado>(c =>
            {
                c.ToTable("CambiosDeEstado");
                c.HasKey(x => x.Id);
                c.Property(x => x.EstadoNombre).HasMaxLength(100);
                c.Ignore(x => x.EstadoActual);

                c.HasOne(x => x.Responsable).WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);
                c.Property(x => x.FechaHoraInicio).IsRequired(false);
                c.Property(x => x.FechaHoraFin).IsRequired(false);
            });

            // ===== SerieTemporal -> Muestras -> Detalles =====
            mb.Entity<SerieTemporal>(s =>
            {
                s.ToTable("SeriesTemporales");
                s.HasKey(x => x.Id);

                s.HasMany(x => x.Muestras)
                    .WithOne()
                    .HasForeignKey(m => m.SerieTemporalId)
                    .OnDelete(DeleteBehavior.Cascade);

                s.HasOne(x => x.Sismografo)
                    .WithMany()
                    .HasForeignKey(x => x.SismografoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<MuestraSismica>(m =>
            {
                m.ToTable("MuestrasSismicas");
                m.HasKey(x => x.Id);

                m.HasMany(x => x.DetalleMuestraSismica)
                    .WithOne()
                    .HasForeignKey(d => d.MuestraSismicaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<DetalleMuestra>(d =>
            {
                d.ToTable("DetallesMuestra");
                d.HasKey(x => x.Id);

                d.HasOne(x => x.TipoDeDato)
                    .WithMany()
                    .HasForeignKey(x => x.TipoDeDatoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<Sismografo>(s =>
            {
                s.ToTable("Sismografos");
                s.HasKey(x => x.Id);

                s.HasOne(x => x.Estacion)
                    .WithMany()
                    .HasForeignKey(x => x.EstacionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Catálogos =====
            mb.Entity<AlcanceSismo>().ToTable("Alcances").HasKey(x => x.Id);
            mb.Entity<ClasificacionSismo>().ToTable("Clasificaciones").HasKey(x => x.Id);
            mb.Entity<OrigenDeGeneracion>().ToTable("Origenes").HasKey(x => x.Id);
            mb.Entity<TipoDeDato>().ToTable("TiposDeDato").HasKey(x => x.Id);

            // ===== Empleado/Usuario =====
            mb.Entity<Usuario>().ToTable("Usuarios").HasKey(x => x.Id);
            mb.Entity<Empleado>().ToTable("Empleados").HasKey(x => x.Id);

            mb.Entity<Empleado>()
                .HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
