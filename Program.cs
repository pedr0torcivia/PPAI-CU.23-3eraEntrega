using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // Necesario para algunas extensiones de EF
using PPAI_2.Infra.Data; // RedSismicaContext, BulkTxtImporter
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PPAI_Revisiones
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show("Error no controlado:\n\n" + Deep(ex ?? new Exception("Desconocido")));
            };

            if (!TryInitializeDatabase(out string initError))
            {
                MessageBox.Show(initError, "Error al inicializar la base", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Aquí, en un entorno real, configurarías la Inyección de Dependencias
            // (ej: ServiceProvider) para que PantallaNuevaRevision reciba sus dependencias.
            // Para este ejemplo de WinForms simple, asume que las dependencias se pasan manualmente
            // o se crean dentro de los controladores.
            Application.Run(new Boundary.PantallaNuevaRevision());
        }

        // CORRECCIÓN 1: Crear una configuración de opciones estáticas para el Contexto.
        private static DbContextOptions<RedSismicaContext> CreateOptions()
        {
            // Puedes cambiar "RedSismicaDB" a un nombre único o usar un archivo SQLite
            return new DbContextOptionsBuilder<RedSismicaContext>()
                .UseInMemoryDatabase(databaseName: "RedSismicaDB_Runtime")
                .Options;
        }

        private static bool TryInitializeDatabase(out string errorMessage)
        {
            errorMessage = string.Empty;

            // CORRECCIÓN 2: Pasamos las opciones al constructor
            var options = CreateOptions();

            try
            {
                using (var ctx = new RedSismicaContext(options))
                {
                    // Crear el esquema de la base de datos (si no existe)
                    ctx.Database.EnsureCreated();

                    // Seed idempotente: usa flag para no reimportar
                    var flag = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import.done");
                    // Nota: Si EventosSismicos es 0, hacemos la importación
                    if (!File.Exists(flag) && !ctx.EventosSismicos.Any())
                    {
                        var importFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import");
                        // CORRECCIÓN 3: BulkTxtImporter.Run ahora es accesible (asumiendo que está en PPAI_2.Infra.Data)
                        // Si 'BulkTxtImporter' es interno (como en tu código), debería ser accesible si este archivo está en el mismo ensamblado.
                        // Si no funciona, mueve 'BulkTxtImporter' a public static class.
                        BulkTxtImporter.Run(ctx, importFolder);
                        File.WriteAllText(flag, "ok");
                    }
                }
                return true;
            }
            catch (DbUpdateException dbex)
            {
                errorMessage = "Error al inicializar la base (DbUpdateException):\n\n" + Deep(dbex);
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Error al inicializar la base:\n\n" + Deep(ex);
                return false;
            }
        }

        private static string Deep(Exception e)
        {
            var msgs = new List<string>();
            for (var cur = e; cur != null; cur = cur.InnerException)
                msgs.Add(cur.GetType().Name + ": " + cur.Message);
            return string.Join("\n→ ", msgs);
        }
    }
}