using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projektverwaltung.Data;
using Projektverwaltung.Models;
using System.Windows.Input;

namespace Projektverwaltung.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Mitarbeiter> Mitarbeitende { get; } = new ObservableCollection<Mitarbeiter>();

        private string _status;
        public string Status { get => _status; set => Set(ref _status, value); }

        public ICommand LoadCommand { get; }

        public MainViewModel()
        {
            LoadCommand = new RelayCommand(Load);
            Status = "Bereit";
        }

        private void Load()
        {
            Mitarbeitende.Clear();
            try
            {
                var repo = new MitarbeiterRepository();
                foreach (var m in repo.GetAll())
                    Mitarbeitende.Add(m);
                Status = $"Geladen: {Mitarbeitende.Count} Mitarbeitende";
            }
            catch (System.Exception ex)
            {
                Status = "DB-Fehler: " + ex.Message;
            }
        }
    }
}
