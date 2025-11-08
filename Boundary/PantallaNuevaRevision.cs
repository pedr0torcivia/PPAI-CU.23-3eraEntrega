using PPAI_2.Infra.Data;
using PPAI_Revisiones.Controladores;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory; // NECESARIO para UseInMemoryDatabase


namespace PPAI_Revisiones.Boundary
{
    public partial class PantallaNuevaRevision : Form
    {
        private ManejadorRegistrarRespuesta manejador;

        public PantallaNuevaRevision()
        {
            InitializeComponent();

            // 1. Configuración de la base de datos en memoria (Resuelve CS1061/UseInMemoryDatabase)
            var options = new DbContextOptionsBuilder<RedSismicaContext>()
                .UseInMemoryDatabase(databaseName: "TemporalStartupDB")
                .Options;

            // 2. Inicialización de la Infraestructura (Contexto y Repositorio)
            // Usamos un bloque para limitar el alcance del contexto de inicialización
            using (var ctx_init = new RedSismicaContext(options))
            {
                // Inicialización del Seed (Impoartación de datos)
                var flag = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import.done");
                if (!File.Exists(flag) && !ctx_init.EventosSismicos.Any())
                {
                    // Asumo que BulkTxtImporter.Run es accesible desde aquí (clase interna o pública)
                    BulkTxtImporter.Run(ctx_init, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import"));
                    File.WriteAllText(flag, "ok");
                }
            }

            // 3. INYECCIÓN DE DEPENDENCIAS AL MANEJADOR (Resuelve CS7036)
            // Creamos las instancias que el Manejador necesita
            var ctx_runtime = new RedSismicaContext(options);
            var repo_runtime = new EventoRepositoryEF(ctx_runtime); // Repositorio usa el Contexto de runtime

            // Instanciamos el Manejador pasando sus dependencias
            manejador = new ManejadorRegistrarRespuesta(ctx_runtime, repo_runtime);
        }

        private void PantallaNuevaRevision_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl != btnIniciarCU)
                    ctrl.Visible = false;
            }

            btnIniciarCU.Left = (this.ClientSize.Width - btnIniciarCU.Width) / 2;
            btnIniciarCU.Top = (this.ClientSize.Height - btnIniciarCU.Height) / 2;
        }

        private void btnIniciarCU_Click(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = true;

            btnIniciarCU.Visible = false;

            manejador.ReiniciarCU(this);
            opcionRegistrarResultadoRevisionManual();
        }

        public void opcionRegistrarResultadoRevisionManual()
        {
            Habilitar();
            manejador.RegistrarNuevaRevision(this);
        }

        public void Habilitar()
        {
            this.Enabled = true;
        }

        public void SolicitarSeleccionEvento(List<object> eventos)
        {
            gridEventos.AutoGenerateColumns = true;
            gridEventos.DataSource = null;
            gridEventos.Columns.Clear();
            gridEventos.DataSource = eventos;

            gridEventos.Refresh();
        }

        private void gridEventos_SelectionChanged(object sender, EventArgs e)
        {
            if (gridEventos.SelectedRows.Count > 0)
            {
                int index = gridEventos.SelectedRows[0].Index;
                TomarSeleccionEvento(index);
            }
        }

        public void TomarSeleccionEvento(int indice)
        {
            manejador.TomarSeleccionEvento(indice, this);
        }

        public void MostrarDetalleEventoSismico(string detalle)
        {
            txtDetalleEvento.Text = detalle;
        }

        public void MostrarSismograma(string contenido)
        {
            try
            {
                var pathIn = contenido?.Trim().Trim('"');

                string fallback = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                                         $"sismograma_fallback_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");

                using (var bmp = new Bitmap(600, 180))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    g.DrawRectangle(Pens.Gray, 10, 10, 580, 160);
                    g.DrawString("Sismograma (fallback)", new Font("Segoe UI", 10), Brushes.Gray, 20, 15);

                    var rnd = new Random();
                    var pts = new System.Drawing.Point[560];
                    for (int i = 0; i < 560; i++)
                    {
                        double t = i * 0.12;
                        double y = Math.Sin(t) * 30 + Math.Sin(t * 0.35) * 10 + (rnd.NextDouble() - .5) * 4;
                        pts[i] = new System.Drawing.Point(20 + i, 90 - (int)y);
                    }
                    using (var p = new Pen(Color.Black, 2))
                        g.DrawLines(p, pts);

                    bmp.Save(fallback, System.Drawing.Imaging.ImageFormat.Png);
                }

                txtSismograma.Visible = false;
                picSismograma.Visible = true;

                if (picSismograma.Image != null)
                {
                    var old = picSismograma.Image;
                    picSismograma.Image = null;
                    old.Dispose();
                }

                using (var fs = new FileStream(fallback, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    picSismograma.Image = Image.FromStream(fs);
                }
                picSismograma.BringToFront();
            }
            catch (Exception ex)
            {
                picSismograma.Visible = false;
                txtSismograma.Visible = true;
                txtSismograma.Text = $"[Error al mostrar sismograma]: {ex.Message}";
            }
        }

        public void opcionMostrarMapa()
        {
            grpMapa.Visible = true;
        }

        public void TomarDecisionVisualizarMapa(bool deseaVerMapa)
        {
            manejador.TomarDecisionVisualizarMapa(deseaVerMapa, this);
        }

        public void OpcionModificacionAlcance()
        {
            grpModificarAlcance.Enabled = true;
        }

        private void rbtnModAlcanceNo_Click(object sender, EventArgs e)
        {
            TomarOpcionModificacionAlcance(false);
        }

        private void rbtnModAlcanceSi_Click(object sender, EventArgs e)
        {
            TomarOpcionModificacionAlcance(true);
        }

        public void TomarOpcionModificacionAlcance(bool modificar)
        {
            manejador.TomarOpcionModificacionAlcance(modificar, this);
        }

        public void OpcionModificacionMagnitud()
        {
            grpModificarMagnitud.Enabled = true;
        }

        private void rbtnModMagnitudNo_Click(object sender, EventArgs e)
        {
            TomarOpcionModificacionMagnitud(false);
        }

        private void rbtnModMagnitudSi_Click(object sender, EventArgs e)
        {
            TomarOpcionModificacionMagnitud(true);
        }

        public void TomarOpcionModificacionMagnitud(bool modificar)
        {
            manejador.TomarOpcionModificacionMagnitud(modificar, this);
        }

        public void OpcionModificacionOrigen()
        {
            grpModificarOrigen.Enabled = true;
        }

        private void rbtnModOrigenNo_Click(object sender, EventArgs e)
        {
            TomarOpcionModificacionOrigen(false);
        }

        private void rbtnModOrigenSi_Click(object sender, EventArgs e)
        {
            TomarOpcionModificacionOrigen(true);
        }

        public void TomarOpcionModificacionOrigen(bool modificar)
        {
            manejador.TomarOpcionModificacionOrigen(modificar, this);
        }

        public void SolicitarSeleccionAcciones()
        {
            cmbAccion.Enabled = true;
            btnConfirmar.Enabled = true;
        }

        public void TomarSelecOpcionAccion()
        {
            int opcion = cmbAccion.SelectedIndex + 1;
            manejador.TomarOpcionAccion(opcion, this);
        }

        public void MostrarMensaje(string texto)
        {
            MessageBox.Show(texto);
        }

        public void MostrarBotonCancelar()
        {
            btnCancelar.Visible = true;
            btnConfirmar.Visible = true;
        }

        public void RestaurarEstadoInicial()
        {
            grpMapa.Visible = false;
            rbtnMapaSi.Checked = false;
            rbtnMapaNo.Checked = false;

            grpModificarAlcance.Enabled = false;
            rbtnModAlcanceSi.Checked = false;
            rbtnModAlcanceNo.Checked = false;

            grpModificarMagnitud.Enabled = false;
            rbtnModMagnitudSi.Checked = false;
            rbtnModMagnitudNo.Checked = false;

            grpModificarOrigen.Enabled = false;
            rbtnModOrigenSi.Checked = false;
            rbtnModOrigenNo.Checked = false;

            cmbAccion.SelectedIndex = -1;
            cmbAccion.Enabled = false;

            btnConfirmar.Enabled = false;
            btnConfirmar.Visible = false;
            btnCancelar.Visible = false;

            txtDetalleEvento.Clear();
            txtSismograma.Clear();
            lblMapa.Text = "";

            if (picSismograma.Image != null)
            {
                var old = picSismograma.Image;
                picSismograma.Image = null;
                old.Dispose();
            }
            picSismograma.Visible = false;
            txtSismograma.Visible = true;

            gridEventos.ClearSelection();
        }

        private void btnMapaSi_Click(object sender, EventArgs e)
        {
            TomarDecisionVisualizarMapa(true);
        }

        private void btnMapaNo_Click(object sender, EventArgs e)
        {
            TomarDecisionVisualizarMapa(false);
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            TomarSelecOpcionAccion();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            manejador.ReiniciarCU(this);
        }

        private void picSismograma_Click(object sender, EventArgs e)
        {

        }

        private void gridEventos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        public bool SeleccionoModificarAlcance => rbtnModAlcanceSi.Checked;
        public bool SeleccionoModificarMagnitud => rbtnModMagnitudSi.Checked;
        public bool SeleccionoModificarOrigen => rbtnModOrigenSi.Checked;
        public bool SeleccionoVisualizarMapa => rbtnMapaSi.Checked;
    }
}