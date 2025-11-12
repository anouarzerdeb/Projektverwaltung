using System.Windows;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class EmployeeForm : Window
    {
        private readonly Db _db = new Db();
        private readonly Employee _editing; // null => Neu

        public EmployeeForm()
        {
            InitializeComponent();
            Title = "Mitarbeiter anlegen";
        }

        public EmployeeForm(Employee existing) : this()
        {
            _editing = existing;
            Title = "Mitarbeiter ändern";

            // Felder füllen
            TxtFirstName.Text = existing.FirstName;
            TxtLastName.Text = existing.LastName;
            TxtDepartment.Text = existing.Department;
            TxtPhone.Text = existing.Phone;
            TxtEmail.Text = existing.Email;
        }

        private void SaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            // einfache Pflichtfeldprüfung
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                MessageBox.Show("Vorname und Nachname sind Pflichtfelder.",
                    "Eingabe prüfen", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_editing == null)
            {
                var eNew = new Employee
                {
                    FirstName = TxtFirstName.Text.Trim(),
                    LastName = TxtLastName.Text.Trim(),
                    Department = TxtDepartment.Text.Trim(),
                    Phone = TxtPhone.Text.Trim(),
                    Email = TxtEmail.Text.Trim()
                };
                _db.AddEmployee(eNew);
            }
            else
            {
                _editing.FirstName = TxtFirstName.Text.Trim();
                _editing.LastName = TxtLastName.Text.Trim();
                _editing.Department = TxtDepartment.Text.Trim();
                _editing.Phone = TxtPhone.Text.Trim();
                _editing.Email = TxtEmail.Text.Trim();

                _db.UpdateEmployee(_editing);
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
