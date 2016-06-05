CREATE TABLE [dbo].[Tasks] (
    [Id]                   INT                IDENTITY (1, 1) NOT NULL,
    [Name]                 NVARCHAR (1000)    NOT NULL,
    [Description]          NVARCHAR (MAX)     NULL,
    [BoardId]              INT                NOT NULL,
    [CreatorUserId]        INT                NOT NULL,
    [CreationDateTime]     DATETIMEOFFSET (7) CONSTRAINT [DF_Tasks_CreationDateTime] DEFAULT (getdate()) NOT NULL,
    [StatusChangeDateTime] DATETIMEOFFSET (7) CONSTRAINT [DF_Tasks_StatusChangeDateTime] DEFAULT (getdate()) NOT NULL,
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
    [TaskStatusId]         INT                NOT NULL,
    [TaskStatusPosition]   INT                NOT NULL,
    [TaskStatusName]       NVARCHAR (50)      NOT NULL,
    [IsDeleted]            BIT                CONSTRAINT [DF_Tasks_IsDeleted] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Tasks_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id]),
    CONSTRAINT [FK_Tasks_BoardsColors] FOREIGN KEY ([ColorId]) REFERENCES [dbo].[BoardsColors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tasks_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tasks_TasksStatus] FOREIGN KEY ([TaskStatusId]) REFERENCES [dbo].[TasksStatuses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tasks_Users] FOREIGN KEY ([ExecutorUserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Tasks_Users_CreatorUserId] FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_BoardId]
    ON [dbo].[Tasks]([BoardId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_ColorId]
    ON [dbo].[Tasks]([ColorId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_ColorPosition]
    ON [dbo].[Tasks]([ColorPosition] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_CreatorUserId]
    ON [dbo].[Tasks]([CreatorUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_ExecutorUserId]
    ON [dbo].[Tasks]([ExecutorUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_IsDeleted]
    ON [dbo].[Tasks]([IsDeleted] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_ProjectId]
    ON [dbo].[Tasks]([ProjectId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_TaskStatusId]
    ON [dbo].[Tasks]([TaskStatusId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tasks_TaskStatusPosition]
    ON [dbo].[Tasks]([TaskStatusPosition] ASC);


GO
CREATE TRIGGER [dbo].[TasksDeleteTrigger]
ON [dbo].[Tasks]
AFTER Delete
as
BEGIN
	SET NOCOUNT ON;
end
