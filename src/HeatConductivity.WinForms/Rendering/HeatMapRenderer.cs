using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using HeatConductivity.ViewModels;

namespace HeatConductivity.WinForms.Rendering;

/// <summary>Превращает кадр температурного поля в картинку (один пиксель на узел сетки).</summary>
internal static class HeatMapRenderer
{
    public static Bitmap Render(HeatMapFrame frame)
    {
        double[,] temperature = frame.Temperature;
        int nx = temperature.GetLength(0);
        int ny = temperature.GetLength(1);

        var bitmap = new Bitmap(nx, ny, PixelFormat.Format32bppArgb);
        var data = bitmap.LockBits(new Rectangle(0, 0, nx, ny), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            int stride = data.Stride;
            var pixels = new byte[stride * ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    var color = GetColor(temperature[i, j], frame.MinTemperature, frame.MaxTemperature);

                    // j растёт вверх, строки картинки — вниз.
                    int offset = (ny - 1 - j) * stride + i * 4;
                    pixels[offset] = color.B;
                    pixels[offset + 1] = color.G;
                    pixels[offset + 2] = color.R;
                    pixels[offset + 3] = 255;
                }
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bitmap;
    }

    /// <summary>Синий (холодно) → зелёный → красный (тепло); формула та же, что была в исходной форме.</summary>
    public static Color GetColor(double value, double min, double max)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) value = min;
        if (Math.Abs(max - min) < 0.0001) max = min + 1.0;

        double norm = (value - min) / (max - min);
        if (double.IsNaN(norm)) norm = 0.0;
        norm = Math.Max(0.0, Math.Min(1.0, norm));

        int r = Math.Clamp((int)(255 * norm), 0, 255);
        int b = Math.Clamp((int)(255 * (1.0 - norm)), 0, 255);
        int g = Math.Clamp((int)(255 * (1.0 - Math.Abs(norm - 0.5) * 2.0)), 0, 255);

        return Color.FromArgb(r, g, b);
    }
}
