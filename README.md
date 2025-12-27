## SQL-Datenbank in Visual Studio (SQL Server Object Explorer) einrichten

Diese Anleitung zeigt, wie die Datenbank **ProjectManagerDb** direkt in **Visual Studio** über den **SQL Server Object Explorer** erstellt wird – **ohne SSMS**.  
Die Datenbank wird dabei komplett über das SQL-Skript angelegt.

---

### 1) SQL Server Object Explorer öffnen
1. **Visual Studio** öffnen  
2. Menü: **View → SQL Server Object Explorer**

Falls es nicht sichtbar ist:
- Prüfen, ob die Workload **“.NET-Desktopentwicklung”** installiert ist
- Falls SQL-Tools fehlen: **Visual Studio Installer → Modify → Individual components → SQL Server Data Tools (SSDT)** installieren

---

### 2) Mit einer SQL-Instanz verbinden (LocalDB)
1. Im **SQL Server Object Explorer**: Rechtsklick auf **SQL Server** → **Add SQL Server**
2. Bei **Server name** eintragen:  
   `(localdb)\MSSQLLocalDB`
3. **Authentication**: **Windows Authentication**
4. **Connect** klicken

---

### 3) Neue Query erstellen
1. Im SQL Server Object Explorer den verbundenen Server aufklappen (z. B. **(localdb)\MSSQLLocalDB**)
2. Rechtsklick auf die Verbindung (den Server) → **New Query**

> Tipp: Falls „New Query“ nicht direkt am Server erscheint, funktioniert es auch über:  
> **Rechtsklick auf Databases → New Query** (je nach Visual-Studio-Version)

---

### 4) SQL-Skript einfügen und ausführen
1. In das geöffnete Query-Fenster das komplette SQL-Skript einfügen (inkl. `CREATE DATABASE ProjectManagerDb;`)
2. Ausführen mit:
   - Button **Execute** (oben im Query-Fenster)  
   **oder**
   - Tastenkombination **Strg + Shift + E**

Wenn alles korrekt ist, erscheint unten eine Erfolgsmeldung wie:
- **Command(s) completed successfully.**

```sql
CREATE DATABASE ProjectManagerDb;
GO

USE ProjectManagerDb;
GO

-- Employees
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName  NVARCHAR(100) NOT NULL,
    Email     NVARCHAR(255) NULL,
    Phone     NVARCHAR(50)  NULL,
    Department NVARCHAR(100) NULL
);
GO

-- Projects
CREATE TABLE Projects (
    ProjectId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    StartDate DATETIME NOT NULL,
    EndDate   DATETIME NOT NULL,
    OwnerEmployeeId INT NOT NULL
);
GO

ALTER TABLE Projects
ADD CONSTRAINT FK_Projects_Employees
    FOREIGN KEY (OwnerEmployeeId)
    REFERENCES Employees(EmployeeId);
GO

-- Phases
CREATE TABLE Phases (
    PhaseId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    [Number] NVARCHAR(50) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Hours INT NOT NULL
);
GO

ALTER TABLE Phases
ADD CONSTRAINT FK_Phases_Projects
    FOREIGN KEY (ProjectId)
    REFERENCES Projects(ProjectId)
    ON DELETE CASCADE;
GO

-- Nummer eindeutig pro Projekt
CREATE UNIQUE INDEX UX_Phases_Project_Number
ON Phases(ProjectId, [Number]);
GO

-- PhaseDependencies
CREATE TABLE PhaseDependencies (
    PhaseId INT NOT NULL,
    PredecessorPhaseId INT NOT NULL,
    CONSTRAINT PK_PhaseDependencies PRIMARY KEY (PhaseId, PredecessorPhaseId)
);
GO

ALTER TABLE PhaseDependencies
ADD CONSTRAINT FK_PhaseDependencies_Phases_Phase
    FOREIGN KEY (PhaseId)
    REFERENCES Phases(PhaseId)
    ON DELETE CASCADE;
GO

ALTER TABLE PhaseDependencies
ADD CONSTRAINT FK_PhaseDependencies_Phases_Predecessor
    FOREIGN KEY (PredecessorPhaseId)
    REFERENCES Phases(PhaseId);
GO
```
---

### 5) Tabellen anzeigen (Refresh)
1. Im SQL Server Object Explorer:
   - Server → **Databases** → **ProjectManagerDb** → **Tables**
2. Wenn die Tabellen nicht sofort sichtbar sind:
   - Rechtsklick auf **Tables** → **Refresh**
   - ggf. auch Rechtsklick auf **ProjectManagerDb** → **Refresh**

Danach sollten diese Tabellen sichtbar sein:
- `Employees`
- `Projects`
- `Phases`
- `PhaseDependencies`

---

### 6) Demo-Daten einfügen:
Im Query-Fenster kann man z. B. testen:
```sql
USE ProjectManagerDb;
GO

/* 1) Employees */
INSERT INTO Employees (FirstName, LastName, Email, Phone, Department) VALUES
('Anna',   'Müller',     'anna.mueller@firma.de',     '+49 30 123456-10', 'Projektmanagement'),
('Lukas',  'Schneider',  'lukas.schneider@firma.de',  '+49 30 123456-11', 'Entwicklung'),
('Fatima', 'Yilmaz',     'fatima.yilmaz@firma.de',    '+49 30 123456-12', 'QA / Testing'),
('Jonas',  'Weber',      'jonas.weber@firma.de',      '+49 30 123456-13', 'UI/UX Design'),
('Sofia',  'Koch',       'sofia.koch@firma.de',       '+49 30 123456-14', 'IT / Infrastruktur'),
('Emre',   'Demir',      'emre.demir@firma.de',       '+49 30 123456-15', 'Support');
GO

/* 2) Projects */
INSERT INTO Projects (Name, Description, StartDate, EndDate, OwnerEmployeeId)
VALUES
('Intranet-Relaunch',
 'Neues Firmen-Intranet (Design, CMS, Rechte, Go-Live).',
 '2026-01-06', '2026-02-28',
 (SELECT TOP 1 EmployeeId FROM Employees WHERE Email = 'anna.mueller@firma.de'));

INSERT INTO Projects (Name, Description, StartDate, EndDate, OwnerEmployeeId)
VALUES
('CRM-Integration',
 'Anbindung CRM an bestehende Systeme inkl. Datenmigration und Schnittstellen.',
 '2026-02-03', '2026-03-31',
 (SELECT TOP 1 EmployeeId FROM Employees WHERE Email = 'lukas.schneider@firma.de'));
GO

/* 3) Phases + Dependencies for Project: Intranet-Relaunch */
DECLARE @P1 INT = (SELECT TOP 1 ProjectId FROM Projects WHERE Name = 'Intranet-Relaunch');
DECLARE @P1_Phases TABLE ([Number] NVARCHAR(50), PhaseId INT);

INSERT INTO Phases (ProjectId, [Number], Title, Hours)
OUTPUT inserted.[Number], inserted.PhaseId INTO @P1_Phases
VALUES
(@P1, 'A', 'Kickoff & Anforderungen',     12),
(@P1, 'B', 'UX/UI Design',               18),
(@P1, 'C', 'Technische Umsetzung (CMS)', 28),
(@P1, 'D', 'Inhalte migrieren',          16),
(@P1, 'E', 'Tests & Bugfixing',          14),
(@P1, 'F', 'Go-Live & Übergabe',          8);

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, a.PhaseId
FROM @P1_Phases a, @P1_Phases c
WHERE a.[Number] = 'A' AND c.[Number] = 'C';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, b.PhaseId
FROM @P1_Phases b, @P1_Phases c
WHERE b.[Number] = 'B' AND c.[Number] = 'C';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT d.PhaseId, c.PhaseId
FROM @P1_Phases c, @P1_Phases d
WHERE c.[Number] = 'C' AND d.[Number] = 'D';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT e.PhaseId, c.PhaseId
FROM @P1_Phases c, @P1_Phases e
WHERE c.[Number] = 'C' AND e.[Number] = 'E';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT e.PhaseId, d.PhaseId
FROM @P1_Phases d, @P1_Phases e
WHERE d.[Number] = 'D' AND e.[Number] = 'E';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT f.PhaseId, e.PhaseId
FROM @P1_Phases e, @P1_Phases f
WHERE e.[Number] = 'E' AND f.[Number] = 'F';
GO

/* 4) Phases + Dependencies for Project: CRM-Integration */
DECLARE @P2 INT = (SELECT TOP 1 ProjectId FROM Projects WHERE Name = 'CRM-Integration');
DECLARE @P2_Phases TABLE ([Number] NVARCHAR(50), PhaseId INT);

INSERT INTO Phases (ProjectId, [Number], Title, Hours)
OUTPUT inserted.[Number], inserted.PhaseId INTO @P2_Phases
VALUES
(@P2, 'A', 'Analyse & Datenmodell',           16),
(@P2, 'B', 'API/Schnittstellen-Design',       14),
(@P2, 'C', 'Implementierung Schnittstellen',  26),
(@P2, 'D', 'Datenmigration (Testlauf)',       18),
(@P2, 'E', 'Abnahme & Schulung',              10),
(@P2, 'F', 'Produktivsetzung',                 6);

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, a.PhaseId
FROM @P2_Phases a, @P2_Phases c
WHERE a.[Number] = 'A' AND c.[Number] = 'C';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, b.PhaseId
FROM @P2_Phases b, @P2_Phases c
WHERE b.[Number] = 'B' AND c.[Number] = 'C';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT d.PhaseId, c.PhaseId
FROM @P2_Phases c, @P2_Phases d
WHERE c.[Number] = 'C' AND d.[Number] = 'D';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT e.PhaseId, d.PhaseId
FROM @P2_Phases d, @P2_Phases e
WHERE d.[Number] = 'D' AND e.[Number] = 'E';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT f.PhaseId, e.PhaseId
FROM @P2_Phases e, @P2_Phases f
WHERE e.[Number] = 'E' AND f.[Number] = 'F';
GO

```

## 7) Projekt starten (Visual Studio)

1. Lösung (`.sln`) in **Visual Studio** öffnen  
2. Falls nötig: NuGet-Pakete automatisch wiederherstellen lassen  
3. Das Projekt **Projektverwaltung** als Startprojekt setzen:
   - Rechtsklick auf Projekt → **Set as Startup Project**
4. Starten mit:
   - **F5** (mit Debugging) oder **Strg + F5** (ohne Debugging)

---

## 8) Connection String prüfen (App.config)

Die Anwendung verbindet sich über folgenden Eintrag:
`ConfigurationManager.ConnectionStrings["Default"]`

In der Datei **App.config** muss ein Connection String mit dem Namen **Default** vorhanden sein.

Beispiel für **LocalDB**:

```xml
<configuration>
  <connectionStrings>
    <add name="Default"
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectManagerDb;Integrated Security=True;TrustServerCertificate=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
