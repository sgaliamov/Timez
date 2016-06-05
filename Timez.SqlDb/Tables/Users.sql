CREATE TABLE [dbo].[Users] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [Nick]             NVARCHAR (50)      NOT NULL,
    [EMail]            NVARCHAR (320)     NOT NULL,
    [Password]         NVARCHAR (50)      NOT NULL,
    [IsConfirmed]      BIT                CONSTRAINT [DF_Users_IsApproved] DEFAULT ((0)) NOT NULL,
    [RegistrationDate] DATETIMEOFFSET (7) CONSTRAINT [DF_Users_RegistrationDate] DEFAULT (getdate()) NOT NULL,
    [TimeZone]         INT                CONSTRAINT [DF_Users_TimeZone] DEFAULT ((0)) NOT NULL,
    [IsAdmin]          BIT                CONSTRAINT [DF_Users_IsAdmin] DEFAULT ((0)) NOT NULL,
    [ConfimKey]        NVARCHAR (50)      NOT NULL,
    [RegistrationType] INT                CONSTRAINT [DF_Users_RegistrationType] DEFAULT ((0)) NOT NULL,
    [RecievOwnEvents]  BIT                CONSTRAINT [DF_Users_RecievOwnEvents] DEFAULT ((1)) NOT NULL,
    [Login]            NVARCHAR (320)     NOT NULL,
	[EmailChangeDate]  DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

