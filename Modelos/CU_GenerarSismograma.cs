using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class CU_GenerarSismograma
{
    // Genera un PNG temporal con una traza simulada y devuelve la RUTA COMPLETA
    public string Ejecutar()
    {
        int width = 1200, height = 300;
        string file = Path.Combine(Path.GetTempPath(),
            $"sismograma_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");

        using (var bmp = new Bitmap(width, height))
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            // Ejes
            using (var axis = new Pen(Color.Gray, 1))
            {
                g.DrawLine(axis, 40, height / 2, width - 10, height / 2);
                g.DrawLine(axis, 40, 10, 40, height - 10);
            }

            // Título/etiquetas
            using (var f = new Font("Segoe UI", 9))
            using (var b = new SolidBrush(Color.Gray))
            {
                g.DrawString("Sismograma (simulado)", f, b, 44, 14);
                g.DrawString("Amplitud", f, b, 5, 5);
                g.DrawString("Tiempo →", f, b, width - 120, height / 2 + 6);
            }

            // Traza simulada
            var rnd = new Random();
            var pts = new PointF[width - 50];
            float mid = height / 2f, scale = height * .35f;
            double f1 = .02, f2 = .05, f3 = .11;
            double p1 = rnd.NextDouble() * Math.PI * 2;
            double p2 = rnd.NextDouble() * Math.PI * 2;
            double p3 = rnd.NextDouble() * Math.PI * 2;

            for (int x = 50; x < width; x++)
            {
                double t = x;
                double y = Math.Sin(f1 * t + p1) * .6 + Math.Sin(f2 * t + p2) * .3 + Math.Sin(f3 * t + p3) * .1
                           + (rnd.NextDouble() - .5) * .15; // ruido
                pts[x - 50] = new PointF(x, (float)(mid - y * scale));
            }

            using (var pen = new Pen(Color.Black, 1.5f))
                g.DrawLines(pen, pts);

            bmp.Save(file, ImageFormat.Png);
        }

        return file; 
    }
}
