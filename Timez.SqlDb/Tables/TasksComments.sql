CREATE TABLE [dbo].[TasksComments] (
    [Id]            INT                IDENTITY (1, 1) NOT NULL,
    [TaskId]        INT                NOT NULL,
    [Comment]       NVARCHAR (MAX)     NOT NULL,
    [AuthorUserId]  INT                NOT NULL,
    [AuthorUser]    NVARCHAR (50)      NOT NULL,
    [ParentId]      INT                NULL,
    [ParentComment] NVARCHAR (MAX)     CONSTRAINT [DF_TasksComments_ParentComment] DEFAULT ('') NOT NULL,
    [CreationDate]  DATETIMEOFFSET (7) CONSTRAINT [DF_TasksComments_CreationDate] DEFAULT (getdate()) NOT NULL,
    [IsDeleted]     BIT                NOT NULL,
    [BoardId]       INT                NOT NULL,
    CONSTRAINT [PK_TasksComments] PRIMARY KEY CLUSTERED ([Id] DESC),
    CONSTRAINT [FK_TasksComments_Tasks] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Tasks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TasksComments_Users] FOREIGN KEY ([AuthorUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TasksComments_AuthorUserId]
    ON [dbo].[TasksComments]([AuthorUserId] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_TasksComments_CreationDate]
    ON [dbo].[TasksComments]([CreationDate] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_TasksComments_TaskId]
    ON [dbo].[TasksComments]([TaskId] DESC);

