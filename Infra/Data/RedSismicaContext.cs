// Infra/Data/RedSismicaContext.cs
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data.EFModels;
using System;
using System.IO;

namespace PPAI_2.Infra.Data
{
    public class RedSismicaContext : DbContext
    {
        public DbSet<EventoSismicoEF> EventosSismicos => Set<EventoSismicoEF>();
        public DbSet<CambioDeEstadoEF> CambiosDeEstado => Set<CambioDeEstadoEF>();
        public DbSet<SerieTemporalEF> SeriesTemporales => Set<SerieTemporalEF>();
        public DbSet<MuestraSismicaEF> Muestras => Set<MuestraSismicaEF>();
        public DbSet<DetalleMuestraEF> DetallesMuestra => Set<DetalleMuestraEF>();
        public DbSet<SismografoEF> Sismografos => Set<SismografoEF>();
        public DbSet<EstacionSismologicaEF> Estaciones => Set<EstacionSismologicaEF>();
        public DbSet<AlcanceSismoEF> Alcances => Set<AlcanceSismoEF>();
        public DbSet<ClasificacionSismoEF> Clasificaciones => Set<ClasificacionSismoEF>();
        public DbSet<OrigenDeGeneracionEF> Origenes => Set<OrigenDeGeneracionEF>();
        public DbSet<TipoDeDatoEF> TiposDeDato => Set<TipoDeDatoEF>();
        public DbSet<UsuarioEF> Usuarios => Set<UsuarioEF>();
        public DbSet<EmpleadoEF> Empleados => Set<EmpleadoEF>();
        public DbSet<MuestraSismicaEF> MuestrasEF => Set<MuestraSismicaEF>();
        public DbSet<DetalleMuestraEF> DetallesMuestraEF => Set<DetalleMuestraEF>();
        public DbSet<TipoDeDatoEF> TiposDeDatoEF => Set<TipoDeDatoEF>();


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redsismica.db");
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // Tablas y claves
            mb.Entity<AlcanceSismoEF>().ToTable("Alcances").HasKey(x => x.Id);
            mb.Entity<ClasificacionSismoEF>().ToTable("Clasificaciones").HasKey(x => x.Id);
            mb.Entity<OrigenDeGeneracionEF>().ToTable("Origenes").HasKey(x => x.Id);
            mb.Entity<TipoDeDatoEF>().ToTable("TiposDeDato").HasKey(x => x.Id);
            mb.Entity<UsuarioEF>().ToTable("Usuarios").HasKey(x => x.Id);
            mb.Entity<EmpleadoEF>().ToTable("Empleados").HasKey(x => x.Id);
            mb.Entity<SismografoEF>().ToTable("Sismografos").HasKey(x => x.Id);
            mb.Entity<EstacionSismologicaEF>().ToTable("Estaciones").HasKey(x => x.Id);

            mb.Entity<EventoSismicoEF>(e =>
            {
                e.ToTable("EventosSismicos");
                e.HasKey(x => x.Id);

                e.HasMany(x => x.CambiosDeEstado)
                 .WithOne(c => c.Evento)
                 .HasForeignKey(c => c.EventoSismicoId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(x => x.SeriesTemporales)
                 .WithOne(s => s.Evento)
                 .HasForeignKey(s => s.EventoSismicoId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Alcance).WithMany().HasForeignKey(x => x.AlcanceId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Clasificacion).WithMany().HasForeignKey(x => x.ClasificacionId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Origen).WithMany().HasForeignKey(x => x.OrigenId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Responsable).WithMany().HasForeignKey(x => x.ResponsableId).OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.FechaHoraInicio);
            });

            mb.Entity<CambioDeEstadoEF>(c =>
            {
                c.ToTable("CambiosDeEstado");
                c.HasKey(x => x.Id);
                c.Property(x => x.Id).ValueGeneratedNever();

                c.HasOne(x => x.Responsable)
                 .WithMany()
                 .HasForeignKey(x => x.ResponsableId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<SerieTemporalEF>(s =>
            {
                s.ToTable("SeriesTemporales");
                s.HasKey(x => x.Id);

                s.HasMany(x => x.Muestras)
                 .WithOne(m => m.SerieTemporal)
                 .HasForeignKey(m => m.SerieTemporalId)
                 .OnDelete(DeleteBehavior.Cascade);

                s.HasOne(x => x.Sismografo)
                 .WithMany()
                 .HasForeignKey(x => x.SismografoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<MuestraSismicaEF>(m =>
            {
                m.ToTable("MuestrasSismicas");
                m.HasKey(x => x.Id);

                m.HasMany(x => x.Detalles)
                 .WithOne(d => d.Muestra)
                 .HasForeignKey(d => d.MuestraSismicaId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<DetalleMuestraEF>(d =>
            {
                d.ToTable("DetallesMuestra");
                d.HasKey(x => x.Id);

                d.HasOne(x => x.TipoDeDato)
                 .WithMany()
                 .HasForeignKey(x => x.TipoDeDatoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<SismografoEF>(s =>
            {
                s.ToTable("Sismografos");
                s.HasKey(x => x.Id);

                s.HasOne(x => x.Estacion)
                 .WithMany()
                 .HasForeignKey(x => x.EstacionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<EmpleadoEF>(e =>
            {
                e.HasOne(x => x.Usuario)
                 .WithOne(u => u.Empleado)
                 .HasForeignKey<EmpleadoEF>(x => x.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
