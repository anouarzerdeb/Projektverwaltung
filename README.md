# Projektverwaltung (WPF) – Project Manager with Gantt Diagram

A WPF desktop application to manage **Employees**, **Projects**, and **Project Phases** with **dependencies** and a generated **Gantt diagram** (including optional slack/buffer visualization). The Gantt chart can be **exported as PNG**.

---

## Features

### Employees
- List employees (read-only grid)
- Create / edit / delete employees
- Delete protection: cannot delete an employee if they are assigned as a project owner

### Projects & Phases
- Create / edit / delete projects
- Each project has: name, start/end date, responsible employee (owner)
- Manage phases inside a project:
  - Phase number (unique per project)
  - Title
  - Duration in hours
  - Multiple predecessors (dependencies)

### Gantt Diagram
- Draws phases as bars on a timeline
- Calculates phase start times based on predecessor relations
- Shows slack/buffer (hatched gray segments) when a successor has multiple predecessors
- Export the complete diagram to **PNG**

---

## Tech Stack
- .NET (WPF)
- SQL Server (LocalDB / Express / full SQL Server)
- ADO.NET (`SqlConnection`, `SqlCommand`)
- Newtonsoft.Json (included in project references)

---

## Prerequisites

### Software
- **Visual Studio 2019/2022** (recommended) with:
  - “.NET desktop development” workload (WPF)
- **SQL Server** (choose one):
  - SQL Server LocalDB (often installed with Visual Studio)
  - SQL Server Express
  - Full SQL Server instance

### Optional
- SQL Server Management Studio (SSMS) to run SQL scripts easily

---

## Database Setup

1) Open SSMS (or any SQL tool connected to your server).

2) Run the following SQL script to create the database and tables:

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
    ON DELETE CASCADE; -- deleting a project deletes its phases
GO

-- Unique phase number per project
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
