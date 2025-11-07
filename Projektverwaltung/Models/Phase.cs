using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektverwaltung.Models
{
    // Phase belongs to one project and can have predecessor phases (by Id)
    public class Phase
    {
        public int PhaseId { get; set; }         // primary key
        public int ProjectId { get; set; }       // FK -> Project
        public string Number { get; set; }       // e.g. "A", "B" (unique inside project)
        public string Title { get; set; }       // phase name
        public int Hours { get; set; }       // duration (in hours, >0)

        public List<int> PredecessorIds { get; set; } // Vorgänger-PhaseIds
    }
}
