// Program.cs
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
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

            Application.Run(new Boundary.PantallaNuevaRevision());
        }

        private static bool TryInitializeDatabase(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                using (var ctx = new RedSismicaContext())
                {
#if DEBUG
                    // En desarrollo: resetea DB
                    ctx.Database.EnsureDeleted();
#endif
                    ctx.Database.EnsureCreated();

                    var importFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import");
                    BulkTxtImporter.Run(ctx, importFolder);

                    // Verificación rápida
                    var evs = ctx.EventosSismicos.Count();
                    var ces = ctx.CambiosDeEstado.Count();
                    MessageBox.Show($"Eventos: {evs}\nCambios: {ces}", "Post-Import");
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
