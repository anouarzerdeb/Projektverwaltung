using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Projektverwaltung.Models;
using System.Configuration;

namespace Projektverwaltung.Data
{
    public class Db
    {
        private readonly string _cs;
        public Db()
        {
            _cs = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }

        // ---------------- Employees ----------------
        public List<Employee> GetEmployees(string query = null)
        {
            var list = new List<Employee>();
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT EmployeeId, FirstName, LastName, Email, Phone, Department
                FROM Employees
                WHERE (@q IS NULL) OR ((FirstName + ' ' + LastName) LIKE '%' + @q + '%')
                ORDER BY LastName, FirstName;", con))
            {
                cmd.Parameters.AddWithValue("@q", (object)query ?? DBNull.Value);
                con.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Employee
                        {
                            EmployeeId = r.GetInt32(0),
                            FirstName = r.GetString(1),
                            LastName = r.GetString(2),
                            Email = r.IsDBNull(3) ? "" : r.GetString(3),
                            Phone = r.IsDBNull(4) ? "" : r.GetString(4),
                            Department = r.IsDBNull(5) ? "" : r.GetString(5)
                        });
            }
            return list;
        }

        public int AddEmployee(Employee e)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                INSERT INTO Employees(FirstName,LastName,Email,Phone,Department)
                OUTPUT INSERTED.EmployeeId
                VALUES(@fn,@ln,@em,@ph,@dep);", con))
            {
                cmd.Parameters.AddWithValue("@fn", e.FirstName);
                cmd.Parameters.AddWithValue("@ln", e.LastName);
                cmd.Parameters.AddWithValue("@em", (object)e.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ph", (object)e.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dep", (object)e.Department ?? DBNull.Value);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void UpdateEmployee(Employee e)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                UPDATE Employees SET FirstName=@fn, LastName=@ln, Email=@em, Phone=@ph, Department=@dep
                WHERE EmployeeId=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", e.EmployeeId);
                cmd.Parameters.AddWithValue("@fn", e.FirstName);
                cmd.Parameters.AddWithValue("@ln", e.LastName);
                cmd.Parameters.AddWithValue("@em", (object)e.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ph", (object)e.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dep", (object)e.Department ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool CanDeleteEmployee(int employeeId)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Projects WHERE OwnerEmployeeId=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", employeeId);
                con.Open();
                return (int)cmd.ExecuteScalar() == 0;
            }
        }

        public void DeleteEmployee(int employeeId)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("DELETE FROM Employees WHERE EmployeeId=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", employeeId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------------- Projects ----------------
        public List<Project> GetProjects()
        {
            var list = new List<Project>();
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT ProjectId, Name, Description, StartDate, EndDate, OwnerEmployeeId
                FROM Projects ORDER BY Name;", con))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Project
                        {
                            ProjectId = r.GetInt32(0),
                            Name = r.GetString(1),
                            Description = r.IsDBNull(2) ? "" : r.GetString(2),
                            StartDate = r.GetDateTime(3),
                            EndDate = r.GetDateTime(4),
                            OwnerEmployeeId = r.GetInt32(5),
                            Phases = new List<Phase>()
                        });
            }
            foreach (var p in list) p.Phases = GetPhases(p.ProjectId);
            return list;
        }

        public Project GetProject(int id)
        {
            Project p = null;
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT ProjectId, Name, Description, StartDate, EndDate, OwnerEmployeeId
                FROM Projects WHERE ProjectId=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                using (var r = cmd.ExecuteReader())
                    if (r.Read())
                        p = new Project
                        {
                            ProjectId = r.GetInt32(0),
                            Name = r.GetString(1),
                            Description = r.IsDBNull(2) ? "" : r.GetString(2),
                            StartDate = r.GetDateTime(3),
                            EndDate = r.GetDateTime(4),
                            OwnerEmployeeId = r.GetInt32(5)
                        };
            }
            if (p != null) p.Phases = GetPhases(p.ProjectId);
            return p;
        }

        public int AddProject(Project p)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                INSERT INTO Projects(Name,Description,StartDate,EndDate,OwnerEmployeeId)
                OUTPUT INSERTED.ProjectId
                VALUES(@n,@d,@s,@e,@o);", con))
            {
                cmd.Parameters.AddWithValue("@n", p.Name);
                cmd.Parameters.AddWithValue("@d", (object)p.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@s", p.StartDate);
                cmd.Parameters.AddWithValue("@e", p.EndDate);
                cmd.Parameters.AddWithValue("@o", p.OwnerEmployeeId);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void UpdateProject(Project p)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                UPDATE Projects SET Name=@n, Description=@d, StartDate=@s, EndDate=@e, OwnerEmployeeId=@o
                WHERE ProjectId=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", p.ProjectId);
                cmd.Parameters.AddWithValue("@n", p.Name);
                cmd.Parameters.AddWithValue("@d", (object)p.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@s", p.StartDate);
                cmd.Parameters.AddWithValue("@e", p.EndDate);
                cmd.Parameters.AddWithValue("@o", p.OwnerEmployeeId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteProject(int id)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("DELETE FROM Projects WHERE ProjectId=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery(); // Phases+deps removed via FK cascade from Projects→Phases and PhaseId cascade
            }
        }

        // ---------------- Phases + dependencies ----------------
        public List<Phase> GetPhases(int projectId)
        {
            var list = new List<Phase>();
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT PhaseId, ProjectId, [Number], Title, Hours
                FROM Phases WHERE ProjectId=@pid ORDER BY [Number];", con))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Phase
                        {
                            PhaseId = r.GetInt32(0),
                            ProjectId = r.GetInt32(1),
                            Number = r.GetString(2),
                            Title = r.GetString(3),
                            Hours = r.GetInt32(4),
                            PredecessorIds = new List<int>()
                        });
            }

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT PhaseId, PredecessorPhaseId
                FROM PhaseDependencies
                WHERE PhaseId IN (SELECT PhaseId FROM Phases WHERE ProjectId=@pid);", con))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    var dict = list.ToDictionary(ph => ph.PhaseId);
                    while (r.Read())
                    {
                        int ph = r.GetInt32(0);
                        int pr = r.GetInt32(1);
                        dict[ph].PredecessorIds.Add(pr);
                    }
                }
            }
            return list;
        }

        public Phase SavePhase(int projectId, int? phaseId, string number, string title, int hours, List<int> predecessorIds)
        {
            if (ExistsPhaseNumber(projectId, number, phaseId))
                throw new Exception("Phasennummer im Projekt bereits vorhanden.");

            int id;
            using (var con = new SqlConnection(_cs))
            {
                con.Open();

                if (phaseId == null || phaseId == 0)
                {
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO Phases(ProjectId,[Number],Title,Hours)
                        OUTPUT INSERTED.PhaseId
                        VALUES(@pid,@num,@tit,@hrs);", con))
                    {
                        cmd.Parameters.AddWithValue("@pid", projectId);
                        cmd.Parameters.AddWithValue("@num", number);
                        cmd.Parameters.AddWithValue("@tit", title);
                        cmd.Parameters.AddWithValue("@hrs", hours);
                        id = (int)cmd.ExecuteScalar();
                    }
                }
                else
                {
                    id = phaseId.Value;
                    using (var cmd = new SqlCommand(@"
                        UPDATE Phases SET [Number]=@num, Title=@tit, Hours=@hrs
                        WHERE PhaseId=@id;", con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@num", number);
                        cmd.Parameters.AddWithValue("@tit", title);
                        cmd.Parameters.AddWithValue("@hrs", hours);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("DELETE FROM PhaseDependencies WHERE PhaseId=@id;", con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (predecessorIds != null && predecessorIds.Count > 0)
                {
                    foreach (var pr in predecessorIds.Distinct().Where(x => x != id))
                    {
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO PhaseDependencies(PhaseId, PredecessorPhaseId)
                            VALUES(@ph,@pr);", con))
                        {
                            cmd.Parameters.AddWithValue("@ph", id);
                            cmd.Parameters.AddWithValue("@pr", pr);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            return GetPhases(projectId).First(x => x.PhaseId == id);
        }

        public void DeletePhase(int phaseId)
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();

                // remove rows where this phase is used as PREDECESSOR (no cascade there)
                using (var cmd = new SqlCommand("DELETE FROM PhaseDependencies WHERE PredecessorPhaseId=@id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", phaseId);
                    cmd.ExecuteNonQuery();
                }
                // delete the phase itself (deps with PhaseId cascade)
                using (var cmd = new SqlCommand("DELETE FROM Phases WHERE PhaseId=@id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", phaseId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool ExistsPhaseNumber(int projectId, string number, int? excludePhaseId)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Phases
                WHERE ProjectId=@pid AND [Number]=@num AND (@ex IS NULL OR PhaseId<>@ex);", con))
            {
                cmd.Parameters.AddWithValue("@pid", projectId);
                cmd.Parameters.AddWithValue("@num", number);
                cmd.Parameters.AddWithValue("@ex", (object)excludePhaseId ?? DBNull.Value);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
