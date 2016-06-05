CREATE TABLE [dbo].[EventHistory] (
    [Id]            INT                IDENTITY (1, 1) NOT NULL,
    [EventDateTime] DATETIMEOFFSET (7) NOT NULL,
    [TaskId]        INT                CONSTRAINT [DF_EventHistory_TaskId] DEFAULT ((0)) NOT NULL,
    [TaskName]      NVARCHAR (MAX)     NOT NULL,
    [UserId]        INT                NOT NULL,
    [UserNick]      NVARCHAR (50)      NOT NULL,
    [Event]         NVARCHAR (MAX)     NOT NULL,
    [EventType]     INT                NOT NULL,
    [ProjectId]     INT                NOT NULL,
    [ProjectName]   NVARCHAR (50)      NOT NULL,
    [BoardId]       INT                NOT NULL,
    CONSTRAINT [PK_EventHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventHistory_EventDateTime]
    ON [dbo].[EventHistory]([EventDateTime] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_EventHistory_EventType]
    ON [dbo].[EventHistory]([EventType] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_EventHistory_ProjectId]
    ON [dbo].[EventHistory]([ProjectId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_EventHistory_UserId]
    ON [dbo].[EventHistory]([UserId] ASC);

