using System;
using System.Windows.Forms;
using PPAI_Revisiones.Boundary;

namespace PPAI_Revisiones
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PantallaNuevaRevision());
        }
    }
}
