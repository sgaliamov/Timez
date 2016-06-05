CREATE TABLE [dbo].[BoardsUsers] (
    [UserId]   INT NOT NULL,
    [BoardId]  INT NOT NULL,
    [UserRole] INT CONSTRAINT [DF_BoardsUsers_UserRole] DEFAULT ((0)) NOT NULL,
    [IsActive] BIT CONSTRAINT [DF_BoardsUsers_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_BoardsUsers] PRIMARY KEY CLUSTERED ([UserId] ASC, [BoardId] ASC),
    CONSTRAINT [FK_BoardsUsers_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BoardsUsers_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_BoardsUsers]
    ON [dbo].[BoardsUsers]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_BoardsUsers_BoardId]
    ON [dbo].[BoardsUsers]([BoardId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_BoardsUsers_IsActive]
    ON [dbo].[BoardsUsers]([IsActive] ASC);

