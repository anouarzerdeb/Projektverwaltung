using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektverwaltung.Models
{
    // Project with a responsible employee and many phases
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int OwnerEmployeeId { get; set; }

        public List<Phase> Phases { get; set; }      // zur Anzeige gefüllt
    }
}
