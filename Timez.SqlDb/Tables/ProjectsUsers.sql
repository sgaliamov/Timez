CREATE TABLE [dbo].[ProjectsUsers] (
    [UserId]      INT NOT NULL,
    [ProjectId]   INT NOT NULL,
    [BoardId]     INT CONSTRAINT [DF_ProjectsUsers_BoardId] DEFAULT ((1)) NOT NULL,
    [ReciveEMail] INT NOT NULL,
    CONSTRAINT [PK_ProjectsUsers] PRIMARY KEY CLUSTERED ([UserId] ASC, [ProjectId] ASC, [BoardId] ASC),
    CONSTRAINT [FK_ProjectsUsers_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id]),
    CONSTRAINT [FK_ProjectsUsers_Projects] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProjectsUsers_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);

