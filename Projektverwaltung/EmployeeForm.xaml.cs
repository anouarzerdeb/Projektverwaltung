using System.Windows;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class EmployeeForm : Window
    {
        private readonly Db _db = new Db();
        private Employee _employee; // null = new

        public EmployeeForm()
        {
            InitializeComponent();
        }

        public EmployeeForm(Employee existing) : this()
        {
            _employee = existing;
            // pre-fill fields
            TxtFirstName.Text = existing.FirstName;
            TxtLastName.Text = existing.LastName;
            TxtDepartment.Text = existing.Department;
            TxtPhone.Text = existing.Phone;
            TxtEmail.Text = existing.Email;
        }

        private void SaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text) || string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                MessageBox.Show("Vorname und Nachname sind Pflicht.", "Hinweis",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_employee == null)
            {
                var e1 = new Employee
                {
                    FirstName = TxtFirstName.Text.Trim(),
                    LastName = TxtLastName.Text.Trim(),
                    Department = TxtDepartment.Text.Trim(),
                    Phone = TxtPhone.Text.Trim(),
                    Email = TxtEmail.Text.Trim()
                };
                e1.EmployeeId = _db.AddEmployee(e1);
            }
            else
            {
                _employee.FirstName = TxtFirstName.Text.Trim();
                _employee.LastName = TxtLastName.Text.Trim();
                _employee.Department = TxtDepartment.Text.Trim();
                _employee.Phone = TxtPhone.Text.Trim();
                _employee.Email = TxtEmail.Text.Trim();
                _db.UpdateEmployee(_employee);
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
