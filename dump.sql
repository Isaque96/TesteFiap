-- dump.sql (SQL Server)

/********************************************************************************************
  1) Schema
********************************************************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'adm')
BEGIN
    EXEC('CREATE SCHEMA adm');
END
GO

/********************************************************************************************
  2) Domain Tables
     - Student, Class, Enrollment
     - User, Role, UserRole, RefreshToken (Identity/Authorization)
     - All tables include CreatedAt and UpdatedAt (UTC). UpdatedAt is kept by AFTER UPDATE triggers.
********************************************************************************************/

-- Student
IF OBJECT_ID('adm.Student', 'U') IS NULL
BEGIN
    CREATE TABLE adm.Student (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,                 -- Nome do aluno (3-100 validação na aplicação)
        BirthDate DATE NOT NULL,                     -- Data de nascimento (validar idade/antiguidade na aplicação)
        Cpf CHAR(11) NOT NULL,                       -- CPF (validar na aplicação)
        Email NVARCHAR(256) NOT NULL,                -- E-mail (validar na aplicação)
        PasswordHash VARBINARY(512) NULL,            -- Hash de senha (BCrypt). Nulo aqui, se não aplicável.
        CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Student PRIMARY KEY (Id)
    );
END

-- Uniqueness: Student(Cpf), Student(Email)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Student_Cpf' AND object_id = OBJECT_ID('adm.Student'))
BEGIN
    CREATE UNIQUE INDEX UX_Student_Cpf ON adm.Student(Cpf);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Student_Email' AND object_id = OBJECT_ID('adm.Student'))
BEGIN
    CREATE UNIQUE INDEX UX_Student_Email ON adm.Student(Email);
END

-- Class
IF OBJECT_ID('adm.Class', 'U') IS NULL
BEGIN
    CREATE TABLE adm.Class (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,                 -- Nome da turma (3-100 validação na aplicação)
        Description NVARCHAR(250) NOT NULL,          -- Descrição (10-250 validação na aplicação)
        CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Class PRIMARY KEY (Id)
    );
END

-- Enrollment
IF OBJECT_ID('adm.Enrollment', 'U') IS NULL
BEGIN
    CREATE TABLE adm.Enrollment (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        StudentId UNIQUEIDENTIFIER NOT NULL,
        ClassId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Enrollment PRIMARY KEY (Id),
        CONSTRAINT FK_Enrollment_Student FOREIGN KEY (StudentId) REFERENCES adm.Student(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Enrollment_Class FOREIGN KEY (ClassId) REFERENCES adm.Class(Id) ON DELETE CASCADE
    );
END

-- Uniqueness: one student cannot enroll twice in the same class
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Enrollment_Student_Class' AND object_id = OBJECT_ID('adm.Enrollment'))
BEGIN
    CREATE UNIQUE INDEX UX_Enrollment_Student_Class ON adm.Enrollment(StudentId, ClassId);
END

-- Role
IF OBJECT_ID('adm.Role', 'U') IS NULL
BEGIN
    CREATE TABLE adm.Role (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,                 -- Ex.: 'Admin'
        Description NVARCHAR(250) NULL,
        CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Role PRIMARY KEY (Id)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Role_Name' AND object_id = OBJECT_ID('adm.Role'))
BEGIN
    CREATE UNIQUE INDEX UX_Role_Name ON adm.Role(Name);
END

-- [User]
IF OBJECT_ID('adm.[User]', 'U') IS NULL
BEGIN
    CREATE TABLE adm.[User] (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(150) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash VARBINARY(512) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_User PRIMARY KEY (Id)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_User_Email' AND object_id = OBJECT_ID('adm.[User]'))
BEGIN
    CREATE UNIQUE INDEX UX_User_Email ON adm.[User](Email);
END

-- UserRole (N:N)
IF OBJECT_ID('adm.UserRole', 'U') IS NULL
BEGIN
    CREATE TABLE adm.UserRole (
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId INT NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_UserRole PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRole_User FOREIGN KEY (UserId) REFERENCES adm.[User](Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES adm.Role(Id) ON DELETE CASCADE
    );
END

-- RefreshToken
IF OBJECT_ID('adm.RefreshToken', 'U') IS NULL
BEGIN
CREATE TABLE adm.RefreshToken (
                                  Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
                                  UserId UNIQUEIDENTIFIER NOT NULL,
                                  Token NVARCHAR(512) NOT NULL,
                                  ExpiresAt DATETIME2(0) NOT NULL,
                                  CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
                                  IsRevoked BIT NOT NULL DEFAULT 0,
                                  RevokedAt DATETIME2(0) NULL,
                                  CONSTRAINT PK_RefreshToken PRIMARY KEY (Id),
                                  CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserId) REFERENCES adm.[User](Id) ON DELETE CASCADE
);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_RefreshToken_Token' AND object_id = OBJECT_ID('adm.RefreshToken'))
BEGIN
CREATE UNIQUE INDEX UX_RefreshToken_Token ON adm.RefreshToken(Token);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RefreshToken_UserId' AND object_id = OBJECT_ID('adm.RefreshToken'))
BEGIN
CREATE INDEX IX_RefreshToken_UserId ON adm.RefreshToken(UserId);
END

/********************************************************************************************
  3) Update tracking via triggers
     - AFTER UPDATE: seta UpdatedAt = SYSUTCDATETIME() quando houver update.
     - Idempotente: drop + create.
********************************************************************************************/

-- Student trigger
IF OBJECT_ID('adm.trg_Student_SetUpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER adm.trg_Student_SetUpdatedAt;

EXEC('
CREATE TRIGGER adm.trg_Student_SetUpdatedAt
ON adm.Student
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s
       SET UpdatedAt = SYSUTCDATETIME()
      FROM adm.Student s
      INNER JOIN inserted i ON s.Id = i.Id;
END
');

-- Class trigger
IF OBJECT_ID('adm.trg_Class_SetUpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER adm.trg_Class_SetUpdatedAt;

EXEC('
CREATE TRIGGER adm.trg_Class_SetUpdatedAt
ON adm.Class
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c
       SET UpdatedAt = SYSUTCDATETIME()
      FROM adm.Class c
      INNER JOIN inserted i ON c.Id = i.Id;
END
');

-- Enrollment trigger
IF OBJECT_ID('adm.trg_Enrollment_SetUpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER adm.trg_Enrollment_SetUpdatedAt;

EXEC('
CREATE TRIGGER adm.trg_Enrollment_SetUpdatedAt
ON adm.Enrollment
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE e
       SET UpdatedAt = SYSUTCDATETIME()
      FROM adm.Enrollment e
      INNER JOIN inserted i ON e.Id = i.Id;
END
');

-- Role trigger
IF OBJECT_ID('adm.trg_Role_SetUpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER adm.trg_Role_SetUpdatedAt;

EXEC('
CREATE TRIGGER adm.trg_Role_SetUpdatedAt
ON adm.Role
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE r
       SET UpdatedAt = SYSUTCDATETIME()
      FROM adm.Role r
      INNER JOIN inserted i ON r.Id = i.Id;
END
');

-- User trigger
IF OBJECT_ID('adm.trg_User_SetUpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER adm.trg_User_SetUpdatedAt;

EXEC('
CREATE TRIGGER adm.trg_User_SetUpdatedAt
ON adm.[User]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
       SET UpdatedAt = SYSUTCDATETIME()
      FROM adm.[User] u
      INNER JOIN inserted i ON u.Id = i.Id;
END
');

-- UserRole trigger
IF OBJECT_ID('adm.trg_UserRole_SetUpdatedAt', 'TR') IS NOT NULL
    DROP TRIGGER adm.trg_UserRole_SetUpdatedAt;

EXEC('
CREATE TRIGGER adm.trg_UserRole_SetUpdatedAt
ON adm.UserRole
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ur
       SET UpdatedAt = SYSUTCDATETIME()
      FROM adm.UserRole ur
      INNER JOIN inserted i
        ON ur.UserId = i.UserId
       AND ur.RoleId = i.RoleId;
END
');

/********************************************************************************************
  4) Seed
     - Default USER
********************************************************************************************/

INSERT INTO [adm].[Role](Name, Description) VALUES ('User', 'Perfil básico de usuário'), ('Admin', 'Perfil de acesso administrador');
INSERT INTO [adm].[User] (Id,Name,Email,PasswordHash,IsActive) VALUES (N'C7CEB797-E794-440E-A111-6BAAC6677A4B',N'Isaque Schuwarte',N'isaque.schuwarte@fiap.com.br',0x243261243131246D3132555662666F4E4E76617A5772302F4542312E2E4B6C4B61516F4B7972424F6C3243724A6669706E772F6F6A37556E3576526D,1);
INSERT INTO [adm].[UserRole](UserId, RoleId) VALUES ((SELECT Id FROM [adm].[User] WHERE Email = 'isaque.schuwarte@fiap.com.br'), (SELECT Id FROM [adm].[Role] WHERE Name = 'Admin'));
