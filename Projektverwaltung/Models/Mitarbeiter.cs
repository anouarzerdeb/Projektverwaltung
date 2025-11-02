using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektverwaltung.Models
{
    internal class Mitarbeiter
    {
        public int Id { get; set; }
        public string Name { get; set; }     // Nachname
        public string Vorname { get; set; }
        public string Abteilung { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
    }
}
