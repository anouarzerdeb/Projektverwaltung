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

### 6) Optional: Kurzer Funktionstest
Im Query-Fenster kann man z. B. testen:
```sql
USE ProjectManagerDb;
SELECT * FROM Employees;
