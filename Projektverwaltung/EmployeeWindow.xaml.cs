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
    public partial class EmployeeWindow : Window
    {
        private readonly Db _db = new Db();
        private Employee _selectedEmployee;

        public EmployeeWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        // Load employees into the DataGrid
        private void LoadEmployees()
        {
            GridEmployees.ItemsSource = _db.GetEmployees(); // Fetch employees from the database
        }

        // Event handler for the "Neu" button (new employee)
        private void NewEmployee_Click(object sender, RoutedEventArgs e)
        {
            var employeeForm = new EmployeeForm(); // Open a form to add a new employee
            employeeForm.ShowDialog();
            LoadEmployees(); // Refresh the list after adding a new employee
        }

        // Event handler for the "Ändern" button (edit employee)
        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null) return; // If no employee is selected, return

            var employeeForm = new EmployeeForm(_selectedEmployee); // Open the form to edit the selected employee
            employeeForm.ShowDialog();
            LoadEmployees(); // Refresh the list after editing the employee
        }

        // Event handler for the "Löschen" button (delete employee)
        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null) return; // If no employee is selected, return

            _db.DeleteEmployee(_selectedEmployee.EmployeeId); // Delete the employee from the database
            LoadEmployees(); // Refresh the list after deleting the employee
        }

        // Event handler for when a row in the DataGrid is selected
        private void GridEmployees_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedEmployee = GridEmployees.SelectedItem as Employee; // Get the selected employee
        }
    }
}
