using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Projektverwaltung.Data;
using Projektverwaltung.Models;
using System.Xml.Linq;

namespace Projektverwaltung
{
    public partial class ProjectForm : Window
    {
        private readonly Db _db = new Db();
        private Project _project;

        public ProjectForm()
        {
            InitializeComponent();
            CmbOwner.ItemsSource = _db.GetEmployees(); // Populate the ComboBox with employees
        }

        // Constructor for editing an existing project
        public ProjectForm(Project project) : this()
        {
            _project = project;
            TxtName.Text = _project.Name;
            TxtStartDate.SelectedDate = _project.StartDate;
            TxtEndDate.SelectedDate = _project.EndDate;
            CmbOwner.SelectedValue = _project.OwnerEmployeeId;
        }

        // Save button click event
        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null) // Adding a new project
            {
                _project = new Project
                {
                    Name = TxtName.Text,
                    StartDate = TxtStartDate.SelectedDate.Value,
                    EndDate = TxtEndDate.SelectedDate.Value,
                    OwnerEmployeeId = (int)CmbOwner.SelectedValue
                };
                _db.AddProject(_project); // Add new project to the database
            }
            else // Editing an existing project
            {
                _project.Name = TxtName.Text;
                _project.StartDate = TxtStartDate.SelectedDate.Value;
                _project.EndDate = TxtEndDate.SelectedDate.Value;
                _project.OwnerEmployeeId = (int)CmbOwner.SelectedValue;
                _db.UpdateProject(_project); // Update the existing project in the database
            }

            Close(); // Close the form
        }

        // Cancel button click event
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Close the form without saving
        }
    }
}
