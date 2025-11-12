using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;     // <— IMPORTANT (fixes Canvas/TextBlock)
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class GanttWindow : Window
    {
        private readonly Db _db = new Db();
        private Project _project;
        private List<Phase> _phases;

        // Layout
        const double RowH = 26;
        const double ColW = 28;
        const double LeftLabelW = 160;

        readonly Brush[] fills = new[]
        {
            (Brush)new SolidColorBrush(Color.FromRgb(0x9E,0xC5,0xE8)),
            (Brush)new SolidColorBrush(Color.FromRgb(0xF0,0xD6,0x9E)),
            (Brush)new SolidColorBrush(Color.FromRgb(0xC9,0xE7,0xC8)),
            (Brush)new SolidColorBrush(Color.FromRgb(0xF2,0xAE,0x73)),
            (Brush)new SolidColorBrush(Color.FromRgb(0xB9,0xD4,0xF0)),
            (Brush)new SolidColorBrush(Color.FromRgb(0xF6,0xC5,0xC5))
        };

        public GanttWindow(Project project)
        {
            InitializeComponent();

            _project = _db.GetProject(project.ProjectId);
            _phases = _db.GetPhases(_project.ProjectId);

            Draw();
        }

        private void Draw()
        {
            var plan = ComputeSchedule(_phases, _project.StartDate.Date);

            int maxDay = plan.Max(p => p.StartOffsetDays + p.DurationDays);
            int rows = plan.Count;

            Axis.Children.Clear();
            Chart.Children.Clear();
            Labels.Children.Clear();

            Axis.Width = LeftLabelW + maxDay * ColW + 20;
            Chart.Width = LeftLabelW + maxDay * ColW + 20;
            Chart.Height = rows * RowH + 10;

            DrawAxis(maxDay);
            DrawGrid(rows, maxDay);

            // linke Beschriftung
            for (int i = 0; i < plan.Count; i++)
            {
                var t = new TextBlock
                {
                    Text = plan[i].Title,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(6, i * RowH + 6, 0, 0)
                };
                Canvas.SetLeft(t, 0);
                Canvas.SetTop(t, i * RowH);
                Labels.Children.Add(t);
            }

            // Balken
            for (int i = 0; i < plan.Count; i++)
            {
                var it = plan[i];
                double x = it.StartOffsetDays * ColW;
                double y = i * RowH + 3;
                double w = Math.Max(ColW * it.DurationDays, 6);
                double h = RowH - 6;

                var rect = new Rectangle
                {
                    Width = w,
                    Height = h,
                    Fill = fills[i % fills.Length],
                    Stroke = (Brush)FindResource("StrokeBrush"),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, LeftLabelW + x);
                Canvas.SetTop(rect, y);
                Chart.Children.Add(rect);

                // stärkerer Rahmen
                var border = new Rectangle
                {
                    Width = w,
                    Height = h,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(border, LeftLabelW + x);
                Canvas.SetTop(border, y);
                Chart.Children.Add(border);

                // optionale Schraffur (letztes Drittel)
                if (it.DurationDays >= 3)
                {
                    var hatch = new Rectangle
                    {
                        Width = w / 3.0,
                        Height = h,
                        Fill = (Brush)FindResource("HatchBrush")
                    };
                    Canvas.SetLeft(hatch, LeftLabelW + x + (2.0 / 3.0) * w);
                    Canvas.SetTop(hatch, y);
                    Chart.Children.Add(hatch);
                }
            }

            LblInfo.Text = $"{_project.Name} • {plan.Count} Phasen • Zeitachse: 1..{maxDay} Tage";
        }

        private void DrawAxis(int maxDay)
        {
            var axisRect = new Rectangle
            {
                Width = LeftLabelW + maxDay * ColW,
                Height = 24,
                Fill = Brushes.White,
                Stroke = (Brush)FindResource("GridBrush"),
                StrokeThickness = 1
            };
            Canvas.SetLeft(axisRect, 0);
            Canvas.SetTop(axisRect, 0);
            Axis.Children.Add(axisRect);

            var lblZeit = new TextBlock
            {
                Text = "Zeit",
                Foreground = (Brush)FindResource("AxisBrush"),
                Margin = new Thickness(6, 3, 0, 0),
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(lblZeit, 0);
            Canvas.SetTop(lblZeit, 2);
            Axis.Children.Add(lblZeit);

            for (int d = 1; d <= maxDay; d++)
            {
                var t = new TextBlock
                {
                    Text = d.ToString(),
                    Foreground = (Brush)FindResource("AxisBrush"),
                    FontSize = 11
                };
                Canvas.SetLeft(t, LeftLabelW + (d - 1) * ColW + 8);
                Canvas.SetTop(t, 4);
                Axis.Children.Add(t);
            }
        }

        private void DrawGrid(int rows, int maxDay)
        {
            for (int d = 0; d <= maxDay; d++)
            {
                var v = new Rectangle { Width = 1, Height = rows * RowH, Fill = (Brush)FindResource("GridBrush") };
                Canvas.SetLeft(v, LeftLabelW + d * ColW);
                Canvas.SetTop(v, 0);
                Chart.Children.Add(v);
            }

            for (int r = 0; r <= rows; r++)
            {
                var h = new Rectangle { Width = maxDay * ColW, Height = 1, Fill = (Brush)FindResource("GridBrush") };
                Canvas.SetLeft(h, LeftLabelW);
                Canvas.SetTop(h, r * RowH);
                Chart.Children.Add(h);
            }
        }

        private List<Item> ComputeSchedule(List<Phase> phases, DateTime projectStart)
        {
            int Days(int hours) => Math.Max(1, (int)Math.Ceiling(hours / 8.0));

            // Topological order (Kahn)
            var indeg = phases.ToDictionary(p => p.PhaseId, _ => 0);
            foreach (var p in phases)
                foreach (var pr in p.PredecessorIds ?? new List<int>())
                    indeg[p.PhaseId]++;

            var q = new Queue<Phase>(phases.Where(p => indeg[p.PhaseId] == 0).OrderBy(p => p.Number));
            var order = new List<Phase>();
            while (q.Count > 0)
            {
                var n = q.Dequeue();
                order.Add(n);
                foreach (var m in phases.Where(x => (x.PredecessorIds ?? new List<int>()).Contains(n.PhaseId)))
                {
                    indeg[m.PhaseId]--;
                    if (indeg[m.PhaseId] == 0) q.Enqueue(m);
                }
            }
            if (order.Count != phases.Count)
                order = phases.OrderBy(p => p.Number).ToList();

            var result = new List<Item>();
            var endById = new Dictionary<int, int>();

            foreach (var p in order)
            {
                int start = 0;
                if (p.PredecessorIds != null && p.PredecessorIds.Count > 0)
                    start = p.PredecessorIds.Where(endById.ContainsKey).Select(id => endById[id]).DefaultIfEmpty(0).Max();

                int dur = Days(p.Hours);
                int end = start + dur;
                endById[p.PhaseId] = end;

                result.Add(new Item
                {
                    PhaseId = p.PhaseId,
                    Number = p.Number,
                    Title = p.Title,
                    DurationDays = dur,
                    StartOffsetDays = start
                });
            }

            return result.OrderBy(i => i.Number).ToList();
        }

        private void ExportPng_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png",
                FileName = $"Gantt_{_project.Name}.png"
            };
            if (dlg.ShowDialog() != true) return;

            double width = Chart.Width;
            double height = Chart.Height + Axis.Height;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, LeftLabelW + width, height + 40));

                // Axis
                var axisBmp = RenderElement(Axis);
                dc.DrawImage(axisBmp, new Rect(LeftLabelW, 0, Axis.Width, Axis.Height));

                // Labels
                foreach (UIElement child in Labels.Children)
                {
                    if (child is TextBlock tb)
                    {
                        var p = new Point(Canvas.GetLeft(tb), Canvas.GetTop(tb) + 40);
                        var ft = new FormattedText(
                            tb.Text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"), 12, Brushes.Black, 1.25);
                        dc.DrawText(ft, p + new Vector(6, 6));
                    }
                }

                // Chart
                var chartBmp = RenderElement(Chart);
                dc.DrawImage(chartBmp, new Rect(0, 40, LeftLabelW + chartBmp.Width, chartBmp.Height));
            }

            var rtb = new RenderTargetBitmap(
                (int)Math.Ceiling(LeftLabelW + width),
                (int)Math.Ceiling(height + 40),
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            using (var fs = File.OpenWrite(dlg.FileName))
            {
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                enc.Save(fs);
            }

            MessageBox.Show("PNG exportiert.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private BitmapSource RenderElement(FrameworkElement fe)
        {
            fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            fe.Arrange(new Rect(fe.DesiredSize));
            var rtb = new RenderTargetBitmap(
                (int)Math.Ceiling(fe.ActualWidth > 0 ? fe.ActualWidth : fe.DesiredSize.Width),
                (int)Math.Ceiling(fe.ActualHeight > 0 ? fe.ActualHeight : fe.DesiredSize.Height),
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(fe);
            return rtb;
        }

        private class Item
        {
            public int PhaseId { get; set; }
            public string Number { get; set; }
            public string Title { get; set; }
            public int DurationDays { get; set; }
            public int StartOffsetDays { get; set; }
        }
    }
}
