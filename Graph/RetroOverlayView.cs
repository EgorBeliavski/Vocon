using Microsoft.Maui.Graphics;

namespace Vocon.Controls;

public class RetroOverlayView : GraphicsView, IDrawable
{
    readonly Random _rng = new();
    float[,]? _noiseGrid;
    const int NoiseCols = 48;
    const int NoiseRows = 64;
    IDispatcherTimer? _noiseTimer;

    public static readonly BindableProperty NoiseOpacityProperty =
        BindableProperty.Create(nameof(NoiseOpacity), typeof(float), typeof(RetroOverlayView), 0.05f);
    public float NoiseOpacity
    {
        get => (float)GetValue(NoiseOpacityProperty);
        set => SetValue(NoiseOpacityProperty, value);
    }

    public static readonly BindableProperty ScanlineOpacityProperty =
        BindableProperty.Create(nameof(ScanlineOpacity), typeof(float), typeof(RetroOverlayView), 0.12f);
    public float ScanlineOpacity
    {
        get => (float)GetValue(ScanlineOpacityProperty);
        set => SetValue(ScanlineOpacityProperty, value);
    }

    public static readonly BindableProperty ScanlineSpacingProperty =
        BindableProperty.Create(nameof(ScanlineSpacing), typeof(float), typeof(RetroOverlayView), 3f);
    public float ScanlineSpacing
    {
        get => (float)GetValue(ScanlineSpacingProperty);
        set => SetValue(ScanlineSpacingProperty, value);
    }

    public RetroOverlayView()
    {
        Drawable = this;
        InputTransparent = true; 
        RegenerateNoise();

        _noiseTimer = Dispatcher.CreateTimer();
        _noiseTimer.Interval = TimeSpan.FromMilliseconds(140);
        _noiseTimer.Tick += (_, _) =>
        {
            RegenerateNoise();
            Invalidate();
        };
        _noiseTimer.Start();
    }

    void RegenerateNoise()
    {
        _noiseGrid = new float[NoiseCols, NoiseRows];
        for (int x = 0; x < NoiseCols; x++)
            for (int y = 0; y < NoiseRows; y++)
                _noiseGrid[x, y] = (float)_rng.NextDouble();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawNoise(canvas, dirtyRect);
        DrawScanlines(canvas, dirtyRect);
    }

    void DrawNoise(ICanvas canvas, RectF rect)
    {
        if (_noiseGrid is null) return;

        float cellW = rect.Width / NoiseCols;
        float cellH = rect.Height / NoiseRows;

        for (int x = 0; x < NoiseCols; x++)
        {
            for (int y = 0; y < NoiseRows; y++)
            {
                float v = _noiseGrid[x, y];
                if (v < 0.82f) continue;

                float a = (v - 0.82f) / 0.18f * NoiseOpacity;
                canvas.FillColor = new Color(1f, 1f, 1f, a);
                canvas.FillRectangle(x * cellW, y * cellH, cellW, cellH);
            }
        }
    }

    void DrawScanlines(ICanvas canvas, RectF rect)
    {
        canvas.StrokeColor = new Color(0f, 0f, 0f, ScanlineOpacity);
        canvas.StrokeSize = 1;

        for (float y = 0; y < rect.Height; y += ScanlineSpacing)
            canvas.DrawLine(0, y, rect.Width, y);
    }

    public void StopAnimating() => _noiseTimer?.Stop();
}