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
    ON DELETE CASCADE; -- wenn Projekt gelöscht wird, auch Phasen löschen
GO

-- Number eindeutig pro Projekt
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
    ON DELETE CASCADE;  -- passt zu deinem Kommentar: Cascade über PhaseId
GO

ALTER TABLE PhaseDependencies
ADD CONSTRAINT FK_PhaseDependencies_Phases_Predecessor
    FOREIGN KEY (PredecessorPhaseId)
    REFERENCES Phases(PhaseId);
GO

/* ============================
   Beispiel-Daten (realistisch)
   für ProjectManagerDb
   ============================ */

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


/* 2) Projects (OwnerEmployeeId per SELECT) */
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

-- Dependencies (C hängt von A und B ab -> hier entsteht oft Slack bei A oder B)
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, a.PhaseId
FROM @P1_Phases a, @P1_Phases c
WHERE a.[Number] = 'A' AND c.[Number] = 'C';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, b.PhaseId
FROM @P1_Phases b, @P1_Phases c
WHERE b.[Number] = 'B' AND c.[Number] = 'C';

-- D hängt von C ab
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT d.PhaseId, c.PhaseId
FROM @P1_Phases c, @P1_Phases d
WHERE c.[Number] = 'C' AND d.[Number] = 'D';

-- E hängt von C und D ab (mehrere Vorgänger -> Slack möglich)
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT e.PhaseId, c.PhaseId
FROM @P1_Phases c, @P1_Phases e
WHERE c.[Number] = 'C' AND e.[Number] = 'E';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT e.PhaseId, d.PhaseId
FROM @P1_Phases d, @P1_Phases e
WHERE d.[Number] = 'D' AND e.[Number] = 'E';

-- F hängt von E ab
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

-- C hängt von A und B ab
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, a.PhaseId
FROM @P2_Phases a, @P2_Phases c
WHERE a.[Number] = 'A' AND c.[Number] = 'C';

INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT c.PhaseId, b.PhaseId
FROM @P2_Phases b, @P2_Phases c
WHERE b.[Number] = 'B' AND c.[Number] = 'C';

-- D hängt von C ab
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT d.PhaseId, c.PhaseId
FROM @P2_Phases c, @P2_Phases d
WHERE c.[Number] = 'C' AND d.[Number] = 'D';

-- E hängt von D ab
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT e.PhaseId, d.PhaseId
FROM @P2_Phases d, @P2_Phases e
WHERE d.[Number] = 'D' AND e.[Number] = 'E';

-- F hängt von E ab
INSERT INTO PhaseDependencies (PhaseId, PredecessorPhaseId)
SELECT f.PhaseId, e.PhaseId
FROM @P2_Phases e, @P2_Phases f
WHERE e.[Number] = 'E' AND f.[Number] = 'F';
GO


/* 5) Quick check (optional) */
SELECT TOP 50 * FROM Employees ORDER BY EmployeeId;
SELECT TOP 50 * FROM Projects  ORDER BY ProjectId;
SELECT TOP 50 * FROM Phases    ORDER BY ProjectId, [Number];
SELECT TOP 50 * FROM PhaseDependencies ORDER BY PhaseId, PredecessorPhaseId;
GO

