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
