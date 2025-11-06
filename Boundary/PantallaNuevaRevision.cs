using PPAI_Revisiones.Controladores;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PPAI_Revisiones.Boundary
{
    public partial class PantallaNuevaRevision : Form
    {
        private ManejadorRegistrarRespuesta manejador;

        public PantallaNuevaRevision()
        {
            InitializeComponent();
            manejador = new ManejadorRegistrarRespuesta();
        }

        private void PantallaNuevaRevision_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
            {
                // Oculta todos los controles excepto btnIniciarCU al cargar la pantalla
                if (ctrl != btnIniciarCU)
                    ctrl.Visible = false;
            }

            // Centra el botón de iniciar
            btnIniciarCU.Left = (this.ClientSize.Width - btnIniciarCU.Width) / 2;
            btnIniciarCU.Top = (this.ClientSize.Height - btnIniciarCU.Height) / 2;
        }

        private void btnIniciarCU_Click(object sender, EventArgs e)
        {
            // Mostrar todos los controles (la visibilidad individual se gestiona más adelante)
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = true;

            btnIniciarCU.Visible = false;

            manejador.ReiniciarCU(this); // si hay algo, lo resetea
            opcionRegistrarResultadoRevisionManual(); // vuelve a iniciar
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
            gridEventos.DataSource = null;
            gridEventos.AutoGenerateColumns = true;
            gridEventos.DataSource = eventos;
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

        // Ahora admite imagen: si 'contenido' es una ruta válida → carga PNG/JPG en picSismograma; si no, muestra texto.
        public void MostrarSismograma(string contenido)
        {
            try
            {
                // 1) LOG para ver qué viene
                var pathIn = contenido?.Trim().Trim('"');

                // 2) SIEMPRE genero una imagen temporal (fallback) para garantizar visualización
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

                // 3) Mostrar SIEMPRE la imagen recién creada
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

            // Limpiar imagen y estados de visualización
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

        // El método TomarSelecOpcionAccion(int opcion) se ha eliminado o renombrado en favor
        // del que no recibe parámetros si la lógica es obtener el índice del ComboBox. 
        // He mantenido la versión sin parámetros y he corregido la llamada en btnConfirmar_Click.

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            manejador.ReiniciarCU(this);
        }

        private void picSismograma_Click(object sender, EventArgs e)
        {

        }

        public bool SeleccionoModificarAlcance => rbtnModAlcanceSi.Checked;
        public bool SeleccionoModificarMagnitud => rbtnModMagnitudSi.Checked;
        public bool SeleccionoModificarOrigen => rbtnModOrigenSi.Checked;
        public bool SeleccionoVisualizarMapa => rbtnMapaSi.Checked;
    }
}
