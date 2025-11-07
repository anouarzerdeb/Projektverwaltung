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
    public partial class EmployeeForm : Window
    {
        private readonly Db _db = new Db();
        private Employee _employee;

        public EmployeeForm()
        {
            InitializeComponent();
        }

        // Constructor for editing an existing employee
        public EmployeeForm(Employee employee) : this()
        {
            _employee = employee;
            TxtFirstName.Text = _employee.FirstName;
            TxtLastName.Text = _employee.LastName;
            TxtDepartment.Text = _employee.Department;
            TxtPhone.Text = _employee.Phone;
        }

        // Save the employee details
        private void SaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_employee == null) // Adding a new employee
            {
                _employee = new Employee
                {
                    FirstName = TxtFirstName.Text,
                    LastName = TxtLastName.Text,
                    Department = TxtDepartment.Text,
                    Phone = TxtPhone.Text
                };
                _db.AddEmployee(_employee); // Add new employee to the database
            }
            else // Editing an existing employee
            {
                _employee.FirstName = TxtFirstName.Text;
                _employee.LastName = TxtLastName.Text;
                _employee.Department = TxtDepartment.Text;
                _employee.Phone = TxtPhone.Text;
                _db.UpdateEmployee(_employee); // Update the existing employee in the database
            }

            Close(); // Close the form
        }

        // Cancel and close the form
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
