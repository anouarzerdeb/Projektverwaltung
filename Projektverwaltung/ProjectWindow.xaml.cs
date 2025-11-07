using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projektverwaltung.Data;
using Projektverwaltung.Models;
using System.Windows;

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

        // Load projects into the DataGrid
        private void LoadProjects()
        {
            GridProjects.ItemsSource = _db.GetProjects(); // Fetch projects from the database
        }

        // Event handler for the "Neu" button (new project)
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var projectForm = new ProjectForm(); // Open a form to add a new project
            projectForm.ShowDialog();
            LoadProjects(); // Refresh the list after adding a new project
        }

        // Event handler for the "Ändern" button (edit project)
        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null) return; // If no project is selected, return

            var projectForm = new ProjectForm(_selectedProject); // Open the form to edit the selected project
            projectForm.ShowDialog();
            LoadProjects(); // Refresh the list after editing the project
        }

        // Event handler for the "Löschen" button (delete project)
        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null) return; // If no project is selected, return

            _db.DeleteProject(_selectedProject.ProjectId); // Delete the project from the database
            LoadProjects(); // Refresh the list after deleting the project
        }

        // Event handler for the "Gantt-Diagramm" button (show Gantt chart)
        private void ShowGantt_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProject == null) return;
            var ganttWindow = new GanttWindow(_selectedProject);
            ganttWindow.Show();
        }

        // Event handler for when a row in the DataGrid is selected
        private void GridProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedProject = GridProjects.SelectedItem as Project; // Get the selected project
        }
    }
}
