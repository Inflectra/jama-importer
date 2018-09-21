using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    /// <summary>
    /// Interaction logic for VistaCanvasControl.xaml
    /// </summary>
    public partial class VistaCanvasControl : UserControl
    {
        public VistaCanvasControl()
        {
            InitializeComponent();

			//#region setup brushes
			//Color color1 = (Color)ColorConverter.ConvertFromString("Blue");
			//Color color2 = (Color)ColorConverter.ConvertFromString("Green");
			//GradientBrush small_gb = new LinearGradientBrush(color1, color2, 135);
			//Color color3 = (Color)ColorConverter.ConvertFromString("LightBlue");
			//Color color4 = (Color)ColorConverter.ConvertFromString("LightGreen");
			//GradientBrush large_gb = new LinearGradientBrush(color3, color4, 135);
            DropShadowEffect orangeGlowEffect = new DropShadowEffect();
            orangeGlowEffect.ShadowDepth = 0;
            orangeGlowEffect.BlurRadius = 10;
            orangeGlowEffect.Color = Colors.Orange;

            this.pathArchBottom.Effect = orangeGlowEffect;
            this.pathLetterS.Effect = orangeGlowEffect;
            this.pathArchTop.Effect = orangeGlowEffect;

			//#endregion

			//int fixX = -185;
			//int fixY = -165;

			//#region small

			//#region bottom
			//for (int i = 0; i < 5; i++)
			//{
			//    PathFigure pf = new PathFigure();
			//    pf.StartPoint = new Point(150 + (i * 15) + fixX, 460 - (i * 5) + fixY);
			//    pf.Segments.Add(new ArcSegment(new Point(300 - (i * 5) + fixX, 300 + (i * 2) + fixY), new Size(500, 1000), 0, false, SweepDirection.Counterclockwise, true));
			//    PathGeometry pg = new PathGeometry();
			//    pg.Figures.Add(pf);
			//    Path p = new Path();
			//    p.Data = pg;
			//    p.StrokeThickness = 1;
			//    p.Stroke = small_gb; // new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
			//    p.StrokeStartLineCap = PenLineCap.Round;
			//    p.StrokeEndLineCap = PenLineCap.Round;
			//    p.BitmapEffect = ogbe;

			//    canvas1.Children.Add(p);
			//}
			//#endregion

			//#region top
			//for (int i = 0; i < 5; i++)
			//{
			//    PathFigure pf = new PathFigure();
			//    pf.StartPoint = new Point(100 + fixX, 400 + (i * 10) + fixY);
			//    pf.Segments.Add(new ArcSegment(new Point(300 - (i * 5) + fixX, 300 + (i * 2) + fixY), new Size(500, 1000), 0, false, SweepDirection.Counterclockwise, true));
			//    PathGeometry pg = new PathGeometry();
			//    pg.Figures.Add(pf);
			//    Path p = new Path();
			//    p.Data = pg;
			//    p.StrokeThickness = 1;
			//    p.Stroke = small_gb;// new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
			//    p.StrokeStartLineCap = PenLineCap.Round;
			//    p.StrokeEndLineCap = PenLineCap.Round;
			//    p.BitmapEffect = ogbe;

			//    canvas1.Children.Add(p);
			//}
			//#endregion
			//#endregion

			//ogbe = new OuterGlowBitmapEffect();
			//ogbe.GlowColor = (Color)(ColorConverter.ConvertFromString("LightBlue"));
			//ogbe.GlowSize = ogbe.GlowSize * 2;

			//#region top
			//for (int i = 0; i < 5; i++)
			//{
			//    PathFigure pf = new PathFigure();
			//    pf.StartPoint = new Point(100 + fixX, 325 + (i * 15) + fixY);
			//    pf.Segments.Add(new ArcSegment(new Point(430 - (i * 15) + fixX, 150 + (i * 6) + fixY), new Size(1000, 2000), 0, false, SweepDirection.Counterclockwise, true));
			//    PathGeometry pg = new PathGeometry();
			//    pg.Figures.Add(pf);
			//    Path p = new Path();
			//    p.Data = pg;
			//    p.StrokeThickness = 1;
			//    p.Stroke = small_gb; // new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
			//    p.StrokeStartLineCap = PenLineCap.Round;
			//    p.StrokeEndLineCap = PenLineCap.Round;
			//    p.BitmapEffect = ogbe;

			//    canvas1.Children.Add(p);
			//}
			//#endregion

			//#region bottom
			//for (int i = 0; i < 5; i++)
			//{
			//    PathFigure pf = new PathFigure();
			//    pf.StartPoint = new Point(100 + (i * 35) + fixX, 525 - (i * 15) + fixY);
			//    pf.Segments.Add(new ArcSegment(new Point(430 - (i * 15) + fixX, 150 + (i * 6) + fixY), new Size(800, 1600), 0, false, SweepDirection.Counterclockwise, true));
			//    PathGeometry pg = new PathGeometry();
			//    pg.Figures.Add(pf);
			//    Path p = new Path();
			//    p.Data = pg;
			//    p.StrokeThickness = 1;
			//    p.Stroke = small_gb; // new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
			//    p.StrokeStartLineCap = PenLineCap.Round;
			//    p.StrokeEndLineCap = PenLineCap.Round;
			//    p.BitmapEffect = ogbe;

			//    canvas1.Children.Add(p);
			//}
			//#endregion
			//#endregion
        }
    }
}
