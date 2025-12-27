using System.Windows;
using System.Windows.Controls;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class ProjectWindow : Window
    {
        private readonly Db _db = new Db();
        private Project _selectedProject;

        public ProjectWindow()
        {
            InitializeComponent();
            LoadProjects();

        }

        private void LoadProjects()
        {
            GridProjects.ItemsSource = _db.GetProjects();
            _selectedProject = null;
            BtnEdit.IsEnabled = BtnDelete.IsEnabled = BtnGantt.IsEnabled = false;
        }

        private void GridProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProject = GridProjects.SelectedItem as Project;
            bool has = _selectedProject != null;
            BtnEdit.IsEnabled = BtnDelete.IsEnabled = BtnGantt.IsEnabled = has;
        }

        private void GridProjects_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selectedProject != null) EditProject_Click(sender, e);
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var f = new ProjectForm { Owner = this };
            if (f.ShowDialog() == true)
            LoadProjects();
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null) return;
            var f = new ProjectForm(_selectedProject) { Owner = this };
            if (f.ShowDialog() == true)
            LoadProjects();
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null) return;

            var r = MessageBox.Show(
                $"Projekt „{_selectedProject.Name}“ wirklich löschen? (Phasen werden mitgelöscht)",
                "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (r == MessageBoxResult.Yes)
            {
                _db.DeleteProject(_selectedProject.ProjectId);
                LoadProjects();
            }
        }

        private void ShowGantt_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null) return;

            // Immer frisch laden (inkl. Phasen + Dependencies)
            var freshProject = _db.GetProject(_selectedProject.ProjectId);

            var gantt = new GanttWindow(freshProject);
            gantt.Owner = this;
            gantt.Show();
        }
    }
}
