using PPAI_Revisiones.Controladores;
using System;
using System.Collections.Generic;
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
                if (ctrl != btnIniciarCU)
                    ctrl.Visible = false;
            }

            btnIniciarCU.Left = (this.ClientSize.Width - btnIniciarCU.Width) / 2;
            btnIniciarCU.Top = (this.ClientSize.Height - btnIniciarCU.Height) / 2;
        }

        private void btnIniciarCU_Click(object sender, EventArgs e)
        {
            // Mostrar todos los controles excepto el botón
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

        public void MostrarSismograma(string contenido)
        {
            txtSismograma.Text = contenido;
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
            btnCancelar.Visible = false;

            txtDetalleEvento.Clear();
            txtSismograma.Clear();
            lblMapa.Text = "";

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
            int opcion = cmbAccion.SelectedIndex + 1;
            TomarSelecOpcionAccion(opcion);
        }

        public void TomarSelecOpcionAccion(int opcion)
        {
            manejador.TomarOpcionAccion(opcion, this);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            manejador.ReiniciarCU(this);
        }

        public bool SeleccionoModificarAlcance => rbtnModAlcanceSi.Checked;
        public bool SeleccionoModificarMagnitud => rbtnModMagnitudSi.Checked;
        public bool SeleccionoModificarOrigen => rbtnModOrigenSi.Checked;
        public bool SeleccionoVisualizarMapa => rbtnMapaSi.Checked;
    }
}
