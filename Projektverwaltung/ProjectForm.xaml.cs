using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Projektverwaltung.Data;
using Projektverwaltung.Models;

namespace Projektverwaltung
{
    public partial class ProjectForm : Window
    {
        private readonly Db _db = new Db();
        private Project _project;                     // aktuelles Projekt (neu/geladen)
        private Phase _phaseEditing;                  // null = add, sonst update
        private ObservableCollection<PhaseRow> _rows; // Anzeige im Grid

        public ProjectForm()
        {
            InitializeComponent();
            CmbOwner.ItemsSource = _db.GetEmployees();
            TogglePhaseSection(false); // erst aktivieren, wenn Projekt gespeichert
        }

        public ProjectForm(Project project) : this()
        {
            _project = _db.GetProject(project.ProjectId); // inkl. Phasen
            TxtName.Text = _project.Name;
            TxtStartDate.SelectedDate = _project.StartDate;
            TxtEndDate.SelectedDate = _project.EndDate;
            CmbOwner.SelectedValue = _project.OwnerEmployeeId;

            LoadPhases();
            TogglePhaseSection(true);
        }

        /* ---------- Projekt speichern / aktualisieren ---------- */
        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateProject()) return;

            if (_project == null) _project = new Project();
            _project.Name = TxtName.Text.Trim();
            _project.StartDate = TxtStartDate.SelectedDate.Value;
            _project.EndDate = TxtEndDate.SelectedDate.Value;
            _project.OwnerEmployeeId = (int)CmbOwner.SelectedValue;

            if (_project.ProjectId == 0)
                _project.ProjectId = _db.AddProject(_project);
            else
                _db.UpdateProject(_project);

            TogglePhaseSection(true);
            LoadPhases();
            MessageBox.Show("Projekt gespeichert.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool ValidateProject()
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                !TxtStartDate.SelectedDate.HasValue ||
                !TxtEndDate.SelectedDate.HasValue ||
                CmbOwner.SelectedValue == null)
            {
                MessageBox.Show("Bitte alle mit * gekennzeichneten Felder ausfüllen.",
                    "Eingabe prüfen", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (TxtEndDate.SelectedDate < TxtStartDate.SelectedDate)
            {
                MessageBox.Show("Enddatum muss nach Startdatum liegen.",
                    "Eingabe prüfen", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private void TogglePhaseSection(bool enabled)
        {
            TxtPhaseNumber.IsEnabled = enabled;
            TxtPhaseTitle.IsEnabled = enabled;
            TxtPhaseHours.IsEnabled = enabled;
            ListPredecessors.IsEnabled = enabled;
            BtnAddOrUpdatePhase.IsEnabled = enabled;
            BtnDeletePhase.IsEnabled = enabled && _phaseEditing != null;
            GridPhases.IsEnabled = enabled;
        }

        /* ---------- Phasen laden & Anzeige vorbereiten ---------- */
        private void LoadPhases()
        {
            if (_project == null || _project.ProjectId == 0)
            {
                _rows = new ObservableCollection<PhaseRow>();
                GridPhases.ItemsSource = _rows;
                ListPredecessors.ItemsSource = new List<Phase>(); // leer
                return;
            }

            var phases = _db.GetPhases(_project.ProjectId);
            _project.Phases = phases;

            // Vorgänger-Auswahlliste (alle Phasen)
            ListPredecessors.ItemsSource = phases.OrderBy(p => p.Number).ToList();

            // Grid-Zeilen aufbereiten
            _rows = new ObservableCollection<PhaseRow>(
                phases.Select(p => new PhaseRow(p, phases))
            );
            GridPhases.ItemsSource = _rows;

            ClearPhaseFields();
        }

        /* ---------- Phasen: Hinzufügen/Aktualisieren ---------- */
        private void AddOrUpdatePhase_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null || _project.ProjectId == 0)
            {
                MessageBox.Show("Bitte zuerst das Projekt speichern.", "Hinweis",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Eingaben prüfen
            if (string.IsNullOrWhiteSpace(TxtPhaseNumber.Text) ||
                string.IsNullOrWhiteSpace(TxtPhaseTitle.Text) ||
                !int.TryParse(TxtPhaseHours.Text, out int hours) || hours <= 0)
            {
                MessageBox.Show("Bitte Nummer, Phase und positive Dauer (Stunden) angeben.",
                    "Eingabe prüfen", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Vorgänger-IDs einsammeln (Mehrfachauswahl)
            var predecessors = ListPredecessors.SelectedItems
                .Cast<Phase>()
                .Select(p => p.PhaseId)
                .ToList();

            try
            {
                var saved = _db.SavePhase(
                    projectId: _project.ProjectId,
                    phaseId: _phaseEditing?.PhaseId,
                    number: TxtPhaseNumber.Text.Trim(),
                    title: TxtPhaseTitle.Text.Trim(),
                    hours: hours,
                    predecessorIds: predecessors);

                LoadPhases(); // neu laden für konsistente Anzeige
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /* ---------- Phasen: Auswahl / Löschen ---------- */
        private void GridPhases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = GridPhases.SelectedItem as PhaseRow;
            if (row == null)
            {
                ClearPhaseFields();
                return;
            }

            _phaseEditing = _project.Phases.First(p => p.PhaseId == row.PhaseId);

            TxtPhaseNumber.Text = _phaseEditing.Number;
            TxtPhaseTitle.Text = _phaseEditing.Title;
            TxtPhaseHours.Text = _phaseEditing.Hours.ToString();

            // Vorgänger-Auswahl setzen
            ListPredecessors.UnselectAll();
            foreach (var ph in _project.Phases)
            {
                if (_phaseEditing.PredecessorIds.Contains(ph.PhaseId))
                    ListPredecessors.SelectedItems.Add(ph);
            }

            LblAddOrUpdate.Text = "Aktualisieren";
            BtnDeletePhase.IsEnabled = true;
        }

        private void DeletePhase_Click(object sender, RoutedEventArgs e)
        {
            var row = GridPhases.SelectedItem as PhaseRow;
            if (row == null) return;

            var r = MessageBox.Show($"Phase „{row.Number} – {row.Title}“ wirklich entfernen?",
                "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;

            _db.DeletePhase(row.PhaseId);
            LoadPhases();
        }

        private void ClearPhaseFields_Click(object sender, RoutedEventArgs e) => ClearPhaseFields();

        private void ClearPhaseFields()
        {
            _phaseEditing = null;
            TxtPhaseNumber.Text = "";
            TxtPhaseTitle.Text = "";
            TxtPhaseHours.Text = "";
            ListPredecessors.UnselectAll();
            LblAddOrUpdate.Text = "Hinzufügen";
            BtnDeletePhase.IsEnabled = false;

            // Vorgänger-Auswahl darf die eigene Phase beim Edit nicht enthalten;
            // nach Clear wieder alle Phasen als potentielle Vorgänger zeigen.
            if (_project?.Phases != null)
                ListPredecessors.ItemsSource = _project.Phases.OrderBy(p => p.Number).ToList();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }

    /* ---------- Hilfs-Klasse für DataGrid ---------- */
    public class PhaseRow
    {
        public int PhaseId { get; }
        public string Number { get; }
        public string Title { get; }
        public int Hours { get; }
        public string PredecessorDisplay { get; }

        public PhaseRow(Phase p, IEnumerable<Phase> all)
        {
            PhaseId = p.PhaseId;
            Number = p.Number;
            Title = p.Title;
            Hours = p.Hours;

            var map = all.ToDictionary(x => x.PhaseId, x => x.Number);
            PredecessorDisplay = (p.PredecessorIds != null && p.PredecessorIds.Count > 0)
                ? string.Join(", ", p.PredecessorIds.Where(map.ContainsKey).Select(id => map[id]))
                : "-";
        }
    }
}
