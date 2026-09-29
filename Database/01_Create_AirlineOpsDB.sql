IF DB_ID(N'AirlineOpsDB') IS NULL
BEGIN
    CREATE DATABASE AirlineOpsDB;
END;
GO

USE AirlineOpsDB;
GO

IF OBJECT_ID(N'dbo.DisruptionTypes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DisruptionTypes
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DisruptionTypes PRIMARY KEY,
        Code VARCHAR(10) NOT NULL CONSTRAINT UQ_DisruptionTypes_Code UNIQUE,
        Description VARCHAR(100) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.DisruptionLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DisruptionLogs
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DisruptionLogs PRIMARY KEY,
        FlightNumber VARCHAR(10) NOT NULL,
        DisruptionTypeId INT NOT NULL,
        DelayMinutes INT NOT NULL,
        RequiresPassengerHotel BIT NOT NULL,
        RequiresMealVoucher BIT NOT NULL,
        Remarks NVARCHAR(500) NULL,
        LoggedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_DisruptionLogs_LoggedAtUtc DEFAULT GETUTCDATE(),
        CONSTRAINT FK_DisruptionLogs_DisruptionTypes FOREIGN KEY (DisruptionTypeId)
            REFERENCES dbo.DisruptionTypes(Id)
    );
END;
GO

INSERT INTO dbo.DisruptionTypes (Code, Description)
SELECT v.Code, v.Description
FROM (VALUES
    ('TECH', 'Technical Delay'),
    ('WX', 'Weather'),
    ('CREW', 'Crew Duty Limit'),
    ('ATC', 'Air Traffic Control'),
    ('OPS', 'Operational Issue')
) AS v(Code, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.DisruptionTypes d WHERE d.Code = v.Code
);
GO
