CREATE TABLE [dbo].[TasksArchive] (
    [ArchiveId]            INT                IDENTITY (1, 1) NOT NULL,
    [Id]                   INT                NOT NULL,
    [Name]                 NVARCHAR (1000)    NOT NULL,
    [Description]          NVARCHAR (MAX)     NULL,
    [BoardId]              INT                NOT NULL,
    [CreatorUserId]        INT                CONSTRAINT [DF_TasksArchive_CreatorUserId] DEFAULT ((1)) NOT NULL,
    [CreationDateTime]     DATETIMEOFFSET (7) CONSTRAINT [DF_TasksArchive_CreationDateTime] DEFAULT (getdate()) NOT NULL,
    [StatusChangeDateTime] DATETIMEOFFSET (7) CONSTRAINT [DF_TasksArchive_StatusChangeDateTime] DEFAULT (getdate()) NOT NULL,
    [PlanningTime]         INT                NULL,
    [ColorId]              INT                NOT NULL,
    [ColorHEX]             NVARCHAR (50)      NOT NULL,
    [ColorName]            NVARCHAR (50)      NOT NULL,
    [ColorPosition]        INT                NOT NULL,
    [ProjectId]            INT                NOT NULL,
    [ProjectName]          NVARCHAR (50)      NOT NULL,
    [ExecutorUserId]       INT                NOT NULL,
    [ExecutorNick]         NVARCHAR (50)      NOT NULL,
    [ExecutorEmail]        NVARCHAR (320)     NOT NULL,
    CONSTRAINT [PK_TasksArchive] PRIMARY KEY CLUSTERED ([ArchiveId] ASC),
    CONSTRAINT [FK_TasksArchive_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id]),
    CONSTRAINT [FK_TasksArchive_BoardsColors] FOREIGN KEY ([ColorId]) REFERENCES [dbo].[BoardsColors] ([Id]),
    CONSTRAINT [FK_TasksArchive_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]),
    CONSTRAINT [FK_TasksArchive_Tasks] FOREIGN KEY ([Id]) REFERENCES [dbo].[Tasks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TasksArchive_Users] FOREIGN KEY ([ExecutorUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TasksArchive_TaskId]
    ON [dbo].[TasksArchive]([Id] ASC);

