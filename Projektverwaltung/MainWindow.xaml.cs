using System.Windows;

namespace Projektverwaltung
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnProjects_Click(object sender, RoutedEventArgs e)
        {
            var w = new ProjectWindow();
            w.Owner = this;
            w.Show();
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            var w = new EmployeeWindow();
            w.Owner = this;
            w.Show();
        }
    }
}
