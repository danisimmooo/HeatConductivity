using System.Drawing.Drawing2D;
using HeatConductivity.ViewModels;
using HeatConductivity.WinForms.Rendering;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Тепловая карта: показывает кадр от модели и сообщает о клике в относительных координатах.</summary>
internal sealed class HeatMapControl : Control
{
    private const float SensorRadius = 4f;

    private HeatMapFrame? _frame;
    private Bitmap? _bitmap;

    public HeatMapControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        Dock = DockStyle.Fill;
        BackColor = Color.Black;
    }

    /// <summary>Клик по карте: (0, 0) — левый верхний угол, (1, 1) — правый нижний.</summary>
    public event Action<double, double>? RelativeClick;

    public HeatMapFrame? Frame
    {
        get => _frame;
        set
        {
            _frame = value;

            _bitmap?.Dispose();
            _bitmap = value == null ? null : HeatMapRenderer.Render(value);

            Invalidate();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (Width > 0 && Height > 0)
            RelativeClick?.Invoke(e.X / (double)Width, e.Y / (double)Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.Clear(BackColor);

        if (_bitmap == null || _frame == null)
            return;

        g.InterpolationMode = InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(_bitmap, ClientRectangle);

        DrawSensor(g, _frame);
    }

    private void DrawSensor(Graphics g, HeatMapFrame frame)
    {
        int nx = frame.Temperature.GetLength(0);
        int ny = frame.Temperature.GetLength(1);

        // Центр узла: узел i занимает полосу [i/nx, (i+1)/nx) по ширине, так же считается клик.
        float x = (frame.SensorX + 0.5f) / nx * Width;
        float y = (ny - 1 - frame.SensorY + 0.5f) / ny * Height;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fill = new SolidBrush(Color.White);
        using var outline = new Pen(Color.Black, 1.5f);
        g.FillEllipse(fill, x - SensorRadius, y - SensorRadius, SensorRadius * 2, SensorRadius * 2);
        g.DrawEllipse(outline, x - SensorRadius, y - SensorRadius, SensorRadius * 2, SensorRadius * 2);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _bitmap?.Dispose();

        base.Dispose(disposing);
    }
}
