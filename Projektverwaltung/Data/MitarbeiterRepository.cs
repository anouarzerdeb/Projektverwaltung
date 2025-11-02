using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projektverwaltung.Models;

namespace Projektverwaltung.Data
{
    public class MitarbeiterRepository
    {
        public IEnumerable<Mitarbeiter> GetAll()
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "SELECT Id, Name, Vorname, Abteilung, Telefon FROM dbo.Mitarbeiter ORDER BY Name", c))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    yield return new Mitarbeiter
                    {
                        Id = r.GetInt32(0),
                        Name = r.IsDBNull(1) ? "" : r.GetString(1),
                        Vorname = r.IsDBNull(2) ? "" : r.GetString(2),
                        Abteilung = r.IsDBNull(3) ? "" : r.GetString(3),
                        Telefon = r.IsDBNull(4) ? "" : r.GetString(4),
                    };
                }
            }
        }
    }
}
