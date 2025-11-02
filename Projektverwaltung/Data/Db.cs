using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Projektverwaltung.Data
{
    public static class Db
    {
        public static SqlConnection Open()
        {
            var cs = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var c = new SqlConnection(cs);
            c.Open();
            return c;
        }
    }
}
