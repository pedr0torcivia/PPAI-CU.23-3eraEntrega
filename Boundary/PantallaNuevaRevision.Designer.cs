namespace PPAI_Revisiones.Boundary
{
    partial class PantallaNuevaRevision
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            gridEventos = new System.Windows.Forms.DataGridView();
            txtDetalleEvento = new System.Windows.Forms.TextBox();
            txtSismograma = new System.Windows.Forms.TextBox();
            picSismograma = new System.Windows.Forms.PictureBox();
            grpMapa = new System.Windows.Forms.GroupBox();
            rbtnMapaNo = new System.Windows.Forms.RadioButton();
            rbtnMapaSi = new System.Windows.Forms.RadioButton();
            grpModificarAlcance = new System.Windows.Forms.GroupBox();
            rbtnModAlcanceNo = new System.Windows.Forms.RadioButton();
            rbtnModAlcanceSi = new System.Windows.Forms.RadioButton();
            grpModificarMagnitud = new System.Windows.Forms.GroupBox();
            rbtnModMagnitudNo = new System.Windows.Forms.RadioButton();
            rbtnModMagnitudSi = new System.Windows.Forms.RadioButton();
            grpModificarOrigen = new System.Windows.Forms.GroupBox();
            rbtnModOrigenNo = new System.Windows.Forms.RadioButton();
            rbtnModOrigenSi = new System.Windows.Forms.RadioButton();
            cmbAccion = new System.Windows.Forms.ComboBox();
            btnConfirmar = new System.Windows.Forms.Button();
            lblMapa = new System.Windows.Forms.Label();
            btnIniciarCU = new System.Windows.Forms.Button();
            btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)gridEventos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSismograma).BeginInit();
            grpMapa.SuspendLayout();
            grpModificarAlcance.SuspendLayout();
            grpModificarMagnitud.SuspendLayout();
            grpModificarOrigen.SuspendLayout();
            SuspendLayout();
            // 
            // gridEventos
            // 
            gridEventos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridEventos.Location = new System.Drawing.Point(9, 12);
            gridEventos.MultiSelect = false;
            gridEventos.Name = "gridEventos";
            gridEventos.Size = new System.Drawing.Size(619, 150);
            gridEventos.TabIndex = 2;
            gridEventos.CellContentClick += gridEventos_CellContentClick;
            gridEventos.SelectionChanged += gridEventos_SelectionChanged;
            // 
            // txtDetalleEvento
            // 
            txtDetalleEvento.Location = new System.Drawing.Point(12, 177);
            txtDetalleEvento.Multiline = true;
            txtDetalleEvento.Name = "txtDetalleEvento";
            txtDetalleEvento.ReadOnly = true;
            txtDetalleEvento.Size = new System.Drawing.Size(310, 467);
            txtDetalleEvento.TabIndex = 3;
            // 
            // txtSismograma
            // 
            txtSismograma.Location = new System.Drawing.Point(22, 12);
            txtSismograma.Multiline = true;
            txtSismograma.Name = "txtSismograma";
            txtSismograma.ReadOnly = true;
            txtSismograma.Size = new System.Drawing.Size(290, 117);
            txtSismograma.TabIndex = 4;
            // 
            // picSismograma
            // 
            picSismograma.Location = new System.Drawing.Point(328, 168);
            picSismograma.Name = "picSismograma";
            picSismograma.Size = new System.Drawing.Size(303, 126);
            picSismograma.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picSismograma.TabIndex = 14;
            picSismograma.TabStop = false;
            picSismograma.Visible = false;
            picSismograma.Click += picSismograma_Click;
            // 
            // grpMapa
            // 
            grpMapa.Controls.Add(rbtnMapaNo);
            grpMapa.Controls.Add(rbtnMapaSi);
            grpMapa.Location = new System.Drawing.Point(331, 300);
            grpMapa.Name = "grpMapa";
            grpMapa.Size = new System.Drawing.Size(300, 50);
            grpMapa.TabIndex = 5;
            grpMapa.TabStop = false;
            grpMapa.Text = "¿Desea visualizar el mapa?";
            grpMapa.Visible = false;
            // 
            // rbtnMapaNo
            // 
            rbtnMapaNo.Location = new System.Drawing.Point(60, 20);
            rbtnMapaNo.Name = "rbtnMapaNo";
            rbtnMapaNo.Size = new System.Drawing.Size(104, 24);
            rbtnMapaNo.TabIndex = 0;
            rbtnMapaNo.Text = "No";
            rbtnMapaNo.CheckedChanged += btnMapaNo_Click;
            // 
            // rbtnMapaSi
            // 
            rbtnMapaSi.Location = new System.Drawing.Point(10, 20);
            rbtnMapaSi.Name = "rbtnMapaSi";
            rbtnMapaSi.Size = new System.Drawing.Size(104, 24);
            rbtnMapaSi.TabIndex = 1;
            rbtnMapaSi.Text = "Sí";
            rbtnMapaSi.CheckedChanged += btnMapaSi_Click;
            // 
            // grpModificarAlcance
            // 
            grpModificarAlcance.Controls.Add(rbtnModAlcanceNo);
            grpModificarAlcance.Controls.Add(rbtnModAlcanceSi);
            grpModificarAlcance.Enabled = false;
            grpModificarAlcance.Location = new System.Drawing.Point(331, 388);
            grpModificarAlcance.Name = "grpModificarAlcance";
            grpModificarAlcance.Size = new System.Drawing.Size(300, 50);
            grpModificarAlcance.TabIndex = 6;
            grpModificarAlcance.TabStop = false;
            grpModificarAlcance.Text = "¿Modificar Alcance?";
            // 
            // rbtnModAlcanceNo
            // 
            rbtnModAlcanceNo.Location = new System.Drawing.Point(60, 20);
            rbtnModAlcanceNo.Name = "rbtnModAlcanceNo";
            rbtnModAlcanceNo.Size = new System.Drawing.Size(104, 24);
            rbtnModAlcanceNo.TabIndex = 0;
            rbtnModAlcanceNo.Text = "No";
            // 
            // rbtnModAlcanceSi
            // 
            rbtnModAlcanceSi.Location = new System.Drawing.Point(10, 20);
            rbtnModAlcanceSi.Name = "rbtnModAlcanceSi";
            rbtnModAlcanceSi.Size = new System.Drawing.Size(104, 24);
            rbtnModAlcanceSi.TabIndex = 1;
            rbtnModAlcanceSi.Text = "Sí";
            // 
            // grpModificarMagnitud
            // 
            grpModificarMagnitud.Controls.Add(rbtnModMagnitudNo);
            grpModificarMagnitud.Controls.Add(rbtnModMagnitudSi);
            grpModificarMagnitud.Enabled = false;
            grpModificarMagnitud.Location = new System.Drawing.Point(331, 444);
            grpModificarMagnitud.Name = "grpModificarMagnitud";
            grpModificarMagnitud.Size = new System.Drawing.Size(300, 50);
            grpModificarMagnitud.TabIndex = 7;
            grpModificarMagnitud.TabStop = false;
            grpModificarMagnitud.Text = "¿Modificar Magnitud?";
            // 
            // rbtnModMagnitudNo
            // 
            rbtnModMagnitudNo.Location = new System.Drawing.Point(60, 20);
            rbtnModMagnitudNo.Name = "rbtnModMagnitudNo";
            rbtnModMagnitudNo.Size = new System.Drawing.Size(104, 24);
            rbtnModMagnitudNo.TabIndex = 0;
            rbtnModMagnitudNo.Text = "No";
            // 
            // rbtnModMagnitudSi
            // 
            rbtnModMagnitudSi.Location = new System.Drawing.Point(10, 20);
            rbtnModMagnitudSi.Name = "rbtnModMagnitudSi";
            rbtnModMagnitudSi.Size = new System.Drawing.Size(104, 24);
            rbtnModMagnitudSi.TabIndex = 1;
            rbtnModMagnitudSi.Text = "Sí";
            // 
            // grpModificarOrigen
            // 
            grpModificarOrigen.Controls.Add(rbtnModOrigenNo);
            grpModificarOrigen.Controls.Add(rbtnModOrigenSi);
            grpModificarOrigen.Enabled = false;
            grpModificarOrigen.Location = new System.Drawing.Point(331, 500);
            grpModificarOrigen.Name = "grpModificarOrigen";
            grpModificarOrigen.Size = new System.Drawing.Size(300, 50);
            grpModificarOrigen.TabIndex = 8;
            grpModificarOrigen.TabStop = false;
            grpModificarOrigen.Text = "¿Modificar Origen?";
            // 
            // rbtnModOrigenNo
            // 
            rbtnModOrigenNo.Location = new System.Drawing.Point(60, 20);
            rbtnModOrigenNo.Name = "rbtnModOrigenNo";
            rbtnModOrigenNo.Size = new System.Drawing.Size(104, 24);
            rbtnModOrigenNo.TabIndex = 0;
            rbtnModOrigenNo.Text = "No";
            // 
            // rbtnModOrigenSi
            // 
            rbtnModOrigenSi.Location = new System.Drawing.Point(10, 20);
            rbtnModOrigenSi.Name = "rbtnModOrigenSi";
            rbtnModOrigenSi.Size = new System.Drawing.Size(104, 24);
            rbtnModOrigenSi.TabIndex = 1;
            rbtnModOrigenSi.Text = "Sí";
            // 
            // cmbAccion
            // 
            cmbAccion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbAccion.FormattingEnabled = true;
            cmbAccion.Items.AddRange(new object[] { "Confirmar Evento", "Rechazar Evento", "Solicitar Revisión a Experto" });
            cmbAccion.Location = new System.Drawing.Point(331, 565);
            cmbAccion.Name = "cmbAccion";
            cmbAccion.Size = new System.Drawing.Size(297, 23);
            cmbAccion.TabIndex = 9;
            // 
            // btnConfirmar
            // 
            btnConfirmar.BackColor = System.Drawing.Color.DodgerBlue;
            btnConfirmar.ForeColor = System.Drawing.Color.White;
            btnConfirmar.Location = new System.Drawing.Point(328, 594);
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.Size = new System.Drawing.Size(160, 50);
            btnConfirmar.TabIndex = 10;
            btnConfirmar.Text = "Confirmar Acción";
            btnConfirmar.UseVisualStyleBackColor = false;
            btnConfirmar.Click += btnConfirmar_Click;
            // 
            // lblMapa
            // 
            lblMapa.Location = new System.Drawing.Point(12, 547);
            lblMapa.Name = "lblMapa";
            lblMapa.Size = new System.Drawing.Size(300, 88);
            lblMapa.TabIndex = 11;
            // 
            // btnIniciarCU
            // 
            btnIniciarCU.Location = new System.Drawing.Point(341, 177);
            btnIniciarCU.Name = "btnIniciarCU";
            btnIniciarCU.Size = new System.Drawing.Size(280, 40);
            btnIniciarCU.TabIndex = 0;
            btnIniciarCU.Text = "Registrar Resultado Revisión Manual";
            btnIniciarCU.UseVisualStyleBackColor = true;
            btnIniciarCU.Click += btnIniciarCU_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BackColor = System.Drawing.Color.Red;
            btnCancelar.ForeColor = System.Drawing.Color.White;
            btnCancelar.Location = new System.Drawing.Point(486, 594);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new System.Drawing.Size(145, 50);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Visible = false;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // PantallaNuevaRevision
            // 
            ClientSize = new System.Drawing.Size(643, 656);
            Controls.Add(btnCancelar);
            Controls.Add(gridEventos);
            Controls.Add(txtDetalleEvento);
            Controls.Add(txtSismograma);
            Controls.Add(picSismograma);
            Controls.Add(grpMapa);
            Controls.Add(grpModificarAlcance);
            Controls.Add(grpModificarMagnitud);
            Controls.Add(grpModificarOrigen);
            Controls.Add(cmbAccion);
            Controls.Add(btnConfirmar);
            Controls.Add(lblMapa);
            Controls.Add(btnIniciarCU);
            Name = "PantallaNuevaRevision";
            Text = "Pantalla Nueva Revisión";
            Load += PantallaNuevaRevision_Load;
            ((System.ComponentModel.ISupportInitialize)gridEventos).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSismograma).EndInit();
            grpMapa.ResumeLayout(false);
            grpModificarAlcance.ResumeLayout(false);
            grpModificarMagnitud.ResumeLayout(false);
            grpModificarOrigen.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.DataGridView gridEventos;
        private System.Windows.Forms.TextBox txtDetalleEvento;
        private System.Windows.Forms.TextBox txtSismograma;
        private System.Windows.Forms.PictureBox picSismograma;
        private System.Windows.Forms.GroupBox grpMapa;
        private System.Windows.Forms.RadioButton rbtnMapaNo;
        private System.Windows.Forms.RadioButton rbtnMapaSi;
        private System.Windows.Forms.GroupBox grpModificarAlcance;
        private System.Windows.Forms.RadioButton rbtnModAlcanceNo;
        private System.Windows.Forms.RadioButton rbtnModAlcanceSi;
        private System.Windows.Forms.GroupBox grpModificarMagnitud;
        private System.Windows.Forms.RadioButton rbtnModMagnitudNo;
        private System.Windows.Forms.RadioButton rbtnModMagnitudSi;
        private System.Windows.Forms.GroupBox grpModificarOrigen;
        private System.Windows.Forms.RadioButton rbtnModOrigenNo;
        private System.Windows.Forms.RadioButton rbtnModOrigenSi;
        private System.Windows.Forms.ComboBox cmbAccion;
        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Label lblMapa;
        private System.Windows.Forms.Button btnIniciarCU;
        private System.Windows.Forms.Button btnCancelar;

    }
}
