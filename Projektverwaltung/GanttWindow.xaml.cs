using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class GanttWindow : Window
    {
        // ----------------- Layout constants -----------------
        private const double LeftOffset = 120;   // Abstand links bis zur Zeitachse
        private const double TopOffset = 25;     // Höhe der Kopfzeile (Zeit)
        private const double RowHeight = 32;     // Höhe pro Phase
        private const double CellWidth = 25;     // Breite einer Zeiteinheit

        // ----------------- Data -----------------
        private readonly Project _project;

        // Ein (Pastell-)Farbpinsel pro Phase, damit jede Phase stabil die gleiche Farbe hat
        private readonly Dictionary<int, SolidColorBrush> _phaseColors = new Dictionary<int, SolidColorBrush>();
        private readonly Random _rand = new Random();

        public GanttWindow(Project project)
        {
            InitializeComponent();
            _project = project ?? throw new ArgumentNullException(nameof(project));
            Title = "Gantt – " + _project.Name;

            DrawGantt();
        }

        // =====================================================
        // 1. Gantt-Diagramm zeichnen
        // =====================================================
        private void DrawGantt()
        {
            CanvasChart.Children.Clear();

            var phases = _project.Phases ?? new List<Phase>();
            if (phases.Count == 0)
                return;

            // Sicherheitsmaßnahme: niemals null bei PredecessorIds
            foreach (var ph in phases)
                ph.PredecessorIds = ph.PredecessorIds ?? new List<int>();

            // 1) Startzeitpunkte aus Abhängigkeiten berechnen
            var startMap = CalculateStartPositions(phases);

            // Spätester Endzeitpunkt (für die Länge der Zeitachse)
            int maxEnd = phases
                .Select(p => startMap[p.PhaseId] + p.Hours)
                .DefaultIfEmpty(1)
                .Max();

            // 2) Canvas-Größe passend setzen
            CanvasChart.Width = LeftOffset + (maxEnd + 1) * CellWidth + 40;
            CanvasChart.Height = TopOffset + phases.Count * RowHeight + 40;

            // Schraffur für Pufferzeiten (grau)
            var hatchBrush = CreateHatchBrush();

            // 3) Zeitachse und vertikales Raster zeichnen
            DrawTimeAxis(maxEnd, phases.Count);

            // 4) Phasenzeilen zeichnen
            for (int rowIndex = 0; rowIndex < phases.Count; rowIndex++)
            {
                var phase = phases[rowIndex];
                double y = TopOffset + rowIndex * RowHeight + 4;

                int start = startMap[phase.PhaseId];
                int end = start + phase.Hours;
                double x = LeftOffset + start * CellWidth;

                // 4.1 Beschriftung links
                DrawPhaseLabel(phase, y);

                // 4.2 farbigen Balken der Phase zeichnen
                DrawPhaseBar(phase, x, y, hatchBrush, startMap, phases, end);
            }
        }

        // =====================================================
        // 2. Startzeiten über Vorgängerbeziehungen berechnen
        // =====================================================
        private Dictionary<int, int> CalculateStartPositions(List<Phase> phases)
        {
            var start = new Dictionary<int, int>();
            var byId = phases.ToDictionary(p => p.PhaseId);

            // Alle Phasen starten initial bei 1
            foreach (var p in phases)
                start[p.PhaseId] = 1;

            // Einfache Relaxations-Schleife:
            // eine Phase darf erst starten, wenn alle Vorgänger beendet sind
            bool changed;
            int safety = 0;

            do
            {
                changed = false;
                safety++;
                if (safety > 1000) break; // Schutz, falls Daten fehlerhaft sind

                foreach (var p in phases)
                {
                    if (p.PredecessorIds == null || p.PredecessorIds.Count == 0)
                        continue;

                    int requiredStart = p.PredecessorIds
                        .Select(id => start[id] + byId[id].Hours) // Ende der Vorgänger
                        .Max();

                    if (requiredStart > start[p.PhaseId])
                    {
                        start[p.PhaseId] = requiredStart;
                        changed = true;
                    }
                }
            } while (changed);

            return start;
        }

        // =====================================================
        // 3. Puffer (Slack) einer Phase bestimmen
        //    -> nur das graue Stück bis zum langsamsten Vorgänger
        // =====================================================
        private int GetSlackLengthForPhase(
            Phase phase,
            List<Phase> allPhases,
            Dictionary<int, int> startMap)
        {
            int slack = 0;
            int endThis = startMap[phase.PhaseId] + phase.Hours;

            // Alle Nachfolger suchen, bei denen diese Phase Vorgänger ist
            foreach (var succ in allPhases.Where(p =>
                         p.PredecessorIds != null &&
                         p.PredecessorIds.Contains(phase.PhaseId)))
            {
                // Alle Vorgänger dieses Nachfolgers
                var predsOfSucc = allPhases
                    .Where(p => succ.PredecessorIds.Contains(p.PhaseId))
                    .ToList();

                // Puffer ist nur interessant, wenn der Nachfolger mehrere Vorgänger hat
                if (predsOfSucc.Count < 2)
                    continue;

                int latestEnd = predsOfSucc
                    .Select(p => startMap[p.PhaseId] + p.Hours)
                    .Max();

                // Nur wenn diese Phase früher fertig ist als der langsamste Vorgänger
                if (endThis < latestEnd)
                {
                    int candidate = latestEnd - endThis;
                    if (candidate > slack)
                        slack = candidate;
                }
            }

            return slack;
        }

        // =====================================================
        // 4. Zeitachse und Raster
        // =====================================================
        private void DrawTimeAxis(int maxEnd, int rowCount)
        {
            // "Zeit" links über den Phasen
            var lblZeit = new TextBlock
            {
                Text = "Zeit",
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(lblZeit, 10);
            Canvas.SetTop(lblZeit, 2);
            CanvasChart.Children.Add(lblZeit);

            // Zahlen 1..maxEnd und vertikale Linien
            for (int t = 1; t <= maxEnd; t++)
            {
                double x = LeftOffset + t * CellWidth;

                var txt = new TextBlock
                {
                    Text = t.ToString(),
                    FontSize = 11
                };
                Canvas.SetLeft(txt, x - 4);
                Canvas.SetTop(txt, 2);
                CanvasChart.Children.Add(txt);

                var gridLine = new Line
                {
                    X1 = x,
                    Y1 = TopOffset,
                    X2 = x,
                    Y2 = TopOffset + rowCount * RowHeight,
                    Stroke = new SolidColorBrush(Color.FromRgb(230, 234, 242)),
                    StrokeThickness = 1
                };
                CanvasChart.Children.Add(gridLine);
            }
        }

        // =====================================================
        // 5. Phasenzeile: Text, farbiger Balken, ggf. Puffer
        // =====================================================

        private void DrawPhaseLabel(Phase phase, double y)
        {
            var lbl = new TextBlock
            {
                Text = phase.Title,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(lbl, 10);
            Canvas.SetTop(lbl, y + 2);
            CanvasChart.Children.Add(lbl);
        }

        private void DrawPhaseBar(
            Phase phase,
            double x,
            double y,
            Brush hatchBrush,
            Dictionary<int, int> startMap,
            List<Phase> allPhases,
            int end)
        {
            // Farbe für diese Phase holen/erzeugen
            SolidColorBrush fill = GetPhaseBrush(phase);
            SolidColorBrush stroke = GetStrokeFromFill(fill);

            // farbiger Balken
            var rect = new Rectangle
            {
                Width = phase.Hours * CellWidth,
                Height = RowHeight - 6,
                RadiusX = 4,
                RadiusY = 4,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            CanvasChart.Children.Add(rect);

            // grauer Puffer, falls vorhanden
            int slackLen = GetSlackLengthForPhase(phase, allPhases, startMap);
            if (slackLen <= 0)
                return;

            double slackX = LeftOffset + end * CellWidth;

            var slackRect = new Rectangle
            {
                Width = slackLen * CellWidth,
                Height = RowHeight - 6,
                RadiusX = 4,
                RadiusY = 4,
                Fill = hatchBrush,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(slackRect, slackX);
            Canvas.SetTop(slackRect, y);
            CanvasChart.Children.Add(slackRect);
        }

        // Zufällige Pastellfarbe pro Phase, aber stabil (wird gemerkt)
        private SolidColorBrush GetPhaseBrush(Phase phase)
        {
            if (_phaseColors.TryGetValue(phase.PhaseId, out var brush))
                return brush;

            byte r = (byte)_rand.Next(140, 220);
            byte g = (byte)_rand.Next(140, 220);
            byte b = (byte)_rand.Next(140, 220);

            brush = new SolidColorBrush(Color.FromArgb(190, r, g, b));
            _phaseColors[phase.PhaseId] = brush;
            return brush;
        }

        // aus der Füllfarbe einen etwas dunkleren Rahmen ableiten
        private SolidColorBrush GetStrokeFromFill(SolidColorBrush fill)
        {
            Color c = fill.Color;
            byte dr = (byte)Math.Max(0, c.R - 40);
            byte dg = (byte)Math.Max(0, c.G - 40);
            byte db = (byte)Math.Max(0, c.B - 40);
            return new SolidColorBrush(Color.FromRgb(dr, dg, db));
        }

        // =====================================================
        // 6. Schraffur-Pinsel für grauen Puffer
        // =====================================================
        private Brush CreateHatchBrush()
        {
            var geo = new GeometryGroup();
            geo.Children.Add(new LineGeometry(new Point(0, 4), new Point(4, 0)));

            var drawing = new GeometryDrawing
            {
                Pen = new Pen(Brushes.Gray, 1),
                Geometry = geo
            };

            return new DrawingBrush(drawing)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 4, 4),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 4, 4),
                ViewboxUnits = BrushMappingMode.Absolute
            };
        }

        // =====================================================
        // 7. Export als PNG (Export-Button im XAML)
        // =====================================================
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PNG-Bild|*.png",
                FileName = "Gantt_" + _project.Name + ".png"
            };

            if (dlg.ShowDialog() != true)
                return;

            // 1) Größe der Canvas ermitteln
            double width = double.IsNaN(CanvasChart.ActualWidth) || CanvasChart.ActualWidth <= 0
                ? CanvasChart.Width
                : CanvasChart.ActualWidth;

            double height = double.IsNaN(CanvasChart.ActualHeight) || CanvasChart.ActualHeight <= 0
                ? CanvasChart.Height
                : CanvasChart.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                MessageBox.Show("Diagramm hat keine gültige Größe zum Exportieren.",
                                "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CanvasChart.Measure(new Size(width, height));
            CanvasChart.Arrange(new Rect(0, 0, width, height));
            CanvasChart.UpdateLayout();

            int pixelWidth = (int)Math.Ceiling(width);
            int pixelHeight = (int)Math.Ceiling(height);

            // 2) Weißer Hintergrund + Canvas in ein RenderTargetBitmap zeichnen
            var rtb = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                96, 96,
                PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                var vb = new VisualBrush(CanvasChart);
                dc.DrawRectangle(vb, null, new Rect(0, 0, width, height));
            }
            rtb.Render(dv);

            // 3) Als PNG speichern
            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = System.IO.File.Create(dlg.FileName))
            {
                png.Save(stream);
            }

            MessageBox.Show("Gantt-Diagramm wurde exportiert.", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
