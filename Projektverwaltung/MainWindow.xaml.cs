using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

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
            var projectWindow = new ProjectWindow();
            projectWindow.Show();
            this.Close(); // Close the MainWindow
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            var employeeWindow = new EmployeeWindow();
            employeeWindow.Show();
            this.Close(); // Close the MainWindow
        }
    }
}
