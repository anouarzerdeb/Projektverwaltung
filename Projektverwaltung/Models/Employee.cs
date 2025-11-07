using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projektverwaltung.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }    // PK
        public string FirstName { get; set; }  // Pflicht
        public string LastName { get; set; }  // Pflicht
        public string Email { get; set; }  // optional
        public string Phone { get; set; }  // optional
        public string Department { get; set; } // optional

        public override string ToString() => LastName + ", " + FirstName;

    }
}
