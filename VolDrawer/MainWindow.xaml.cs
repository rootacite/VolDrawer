using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpDX.Mathematics.Interop;
using ShinenginePlus;

using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using ShinenginePlus.DrawableControls;
using Ellipse = SharpDX.Direct2D1.Ellipse;
using SolidColorBrush = SharpDX.Direct2D1.SolidColorBrush;
using TextAlignment = SharpDX.DirectWrite.TextAlignment;

namespace VolDrawer
{

    public static class RawVector2Ext
    {
        public static float magnitude(this RawVector2 v)
        {
            return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        }
    }
    
    public struct Polar2
    {
        public double θ; 
        public double ρ; //弧度

        public RawVector2 ToVector()
        {
            return new RawVector2(MathF.Cos((float)θ) * (float)ρ, MathF.Sin((float)θ) * (float)ρ);
        }

        static public Polar2 FromVector(RawVector2 Origin)
        {
            double dρ = 0;

            if (Origin.Y > 0) 
                dρ += Math.Acos(Origin.X / Origin.magnitude());
            else
                dρ -= Math.Acos(Origin.X / Origin.magnitude());

            return new Polar2()
            {
                ρ = Origin.magnitude(),
                θ = dρ
            };
        }
        public static Polar2 operator +(Polar2 a, Polar2 b)
        {
            return new Polar2()
            {
                ρ = b.ρ + a.ρ,
                θ = b.θ + a.θ
            };
        }
        public static Polar2 operator -(Polar2 a, Polar2 b)
        {
            return new Polar2()
            {
                ρ = a.ρ - b.ρ,
                θ = a.θ - b.θ
            };
        }

        public Polar2(double θ, double ρ)
        {
            this.θ = θ;
            this.ρ = ρ;
        }

        static public double r2d(double r)
        {
            return r / Math.PI * 180d;
        }

        static public double d2r(double d)
        {
            return d / 180d * Math.PI;
        }
    }
    class RawLine
    {
        private RawVector2 Origin;
        private readonly DeviceContext DC;
        private readonly SolidColorBrush Brush;

        public RawLine(DeviceContext DC, RawColor4 Color)
        {
            this.DC = DC;
            Brush = new SolidColorBrush(DC, Color);
        }

        public void Begin(float x, float y)
        {
            Origin = new RawVector2(x, y);
        }
        public void Begin(RawVector2 p)
        {
            Origin = p;
        }
        public void LineTo(float x, float y, float w = 4.5f)
        {
            DC.DrawLine(Origin, new RawVector2(x, y), Brush, w);
            Origin = new RawVector2(x, y);
        }

        public void LineTo(float x, float y, RawColor4 Color, float w = 4.5f)
        {
            using (SolidColorBrush B = new SolidColorBrush(DC, Color))
                DC.DrawLine(Origin, new RawVector2(x, y), B, w);
            Origin = new RawVector2(x, y);
        }

        public void LineTo(RawVector2 p, float w = 4.5f)
        {
            DC.DrawLine(Origin, p, Brush, w);
            Origin = p;
        }
    }
    
    public partial class MainWindow : Window
    {
        public int[] Line1 = new int[1920];
        public int[] Line2 = new int[1920];

        public bool Ignore = false;
        private RawLine Line;
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 1920; i++)
            {
                Line1[i] = 300;
                Line2[i] = 600;
            }
        }

        private ShinenginePlus.DrawableControls.BitmapImage Image = null;
        private ShinenginePlus.DrawableControls.RenderableImage ImageObj = null;
        private ShinenginePlus.DrawableControls.DrawableText Text = null;
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Image = new ("Back.png");
            
            Plane.DrawProc += (s) =>
            {
                s.View.BeginDraw();
                s.View.Clear(new RawColor4(1,0.9f,0.9f,1));
                ImageObj.Render();
                if (!Ignore)
                {
                    Line.Begin(0, 1080 - Line1[0]);
                    for (int i = 1; i < 1920; i++)
                    {
                        if (Line1[i] != -1)
                            Line.LineTo(i, 1080 - Line1[i], 3.2f);
                    }
                }

                Point p = new Point(0, 0);
                Dispatcher.Invoke(() =>
                {
                    p = Mouse.GetPosition(Plane);
                });
                if (p.X > 0 && p.X < 1920)
                {
                    using (SolidColorBrush sb = new SolidColorBrush(s.View, new RawColor4(1, 0, 0, 0.45f)))
                        s.View.FillEllipse(new Ellipse(new RawVector2((int)p.X, 1080 - Line1[(int)p.X]), 7.5f, 7.5f),
                            sb);

                    double ori = p.X / 1920d * 3.3d;
                    double res = Line1[(int)p.X] / 1080d * 3.3d;

                    Text.text = "(" + ori.ToString("0.00") + "V" + ","+res.ToString("0.00") + "V" + ")";

                    RawVector2 OriginPoint = new RawVector2((int)p.X, 1080 - Line1[(int)p.X]);
                    RawVector2 ToWard = new RawVector2((float)(p.X - OriginPoint.X), (float)(p.Y - OriginPoint.Y));

                    ToWard.X /= ToWard.magnitude();
                    ToWard.Y /= ToWard.magnitude();

                    ToWard.X *= 25;
                    ToWard.Y *= 25;

                    OriginPoint.X += ToWard.X;
                    OriginPoint.Y += ToWard.Y;
                    
                    Text.Range = new RawRectangleF(OriginPoint.X - 100, OriginPoint.Y - 15, OriginPoint.X + 100, OriginPoint.Y + 15);
                    Text.Render();
                }

                /*      Line.Begin(0, 1080 - Line2[0]);
                        for (int i = 1; i < 1920; i ++)
                        {
                            Line.LineTo(i, 1080 - Line2[i], new RawColor4(0, 1, 0, 1), 3.2f);
                        }
                */
                s.View.EndDraw();

                
                return DrawProcResult.Normal;
            };
            Plane.FirstDraw += (s, v) =>
            {
                ImageObj = new(Image, s.View);
            };
            Plane.DrawStartup();
            Plane.m_d2d_info.View.AntialiasMode = AntialiasMode.PerPrimitive;
            Line = new RawLine(Plane.m_d2d_info.View, new RawColor4(0, 0, 0, 0.5f));
            Text = new DrawableText("(0,0)", "Tahoma", 24, Plane.m_d2d_info.View);
            
            Text.TextAlignment = TextAlignment.Center;
            Text.Color = new RawColor4(1,0,0,0.45f);
            
            Contrller Ctrl = new Contrller();
            Ctrl._mainWindow = this;
            Ctrl.Show();

            this.Closing += (g, j) =>
            {
                Ctrl.CanClose = true;
                Ctrl.Close();
            };
        }
    }
}