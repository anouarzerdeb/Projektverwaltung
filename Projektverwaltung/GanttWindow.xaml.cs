using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projektverwaltung.Models;
using System.Windows;

namespace Projektverwaltung
{
    public partial class GanttWindow : Window
    {
        private readonly Project _project;

        public GanttWindow(Project project)
        {
            InitializeComponent();
            _project = project;
            DrawGanttChart();
        }

        private void DrawGanttChart()
        {
            // Placeholder for Gantt chart rendering logic
            // This will be where you draw the project phases on the Gantt canvas
        }
    }
}
