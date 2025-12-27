using System.Windows;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class EmployeeWindow : Window
    {
        private readonly Db _db = new Db();
        private Employee _selected;

        public EmployeeWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            GridEmployees.ItemsSource = _db.GetEmployees();
            BtnEdit.IsEnabled = BtnDelete.IsEnabled = false;
            _selected = null;
        }

        private void GridEmployees_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selected = GridEmployees.SelectedItem as Employee;
            bool has = _selected != null;
            BtnEdit.IsEnabled = has;
            BtnDelete.IsEnabled = has;
        }

        private void GridEmployees_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selected != null) EditEmployee_Click(sender, e);
        }

        private void NewEmployee_Click(object sender, RoutedEventArgs e)
        {
            var f = new EmployeeForm { Owner = this };
            if (f.ShowDialog() == true)
            LoadEmployees();
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;
            var f = new EmployeeForm(_selected) { Owner = this };
            if (f.ShowDialog() == true)
            LoadEmployees();
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            // Safety check: not allowed to delete if used as project owner
            if (!_db.CanDeleteEmployee(_selected.EmployeeId))
            {
                MessageBox.Show("Dieser Mitarbeiter ist einem Projekt zugeordnet. "
                              + "Bitte zuerst die Projektverantwortung ändern.",
                                "Löschen nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var r = MessageBox.Show($"Mitarbeiter „{_selected.LastName}, {_selected.FirstName}“ wirklich löschen?",
                                    "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r == MessageBoxResult.Yes)
            {
                _db.DeleteEmployee(_selected.EmployeeId);
                LoadEmployees();
            }
        }
    }
}
