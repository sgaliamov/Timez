CREATE TABLE [dbo].[Tariffs] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (50) NOT NULL,
    [Price]            MONEY         CONSTRAINT [DF_Tariffs_Price] DEFAULT ((0)) NOT NULL,
    [BoardsCount]      INT           CONSTRAINT [DF_Tariffs_BoardsCount] DEFAULT ((2)) NULL,
    [ProjectsPerBoard] INT           CONSTRAINT [DF_Tariffs_ProjectsPerBoard] DEFAULT ((10)) NULL,
    [EmployeesCount]   INT           CONSTRAINT [DF_Tariffs_EmployeesCount] DEFAULT ((5)) NULL,
    [Flags]            INT           CONSTRAINT [DF_Tariffs_Flags] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Tariffs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

