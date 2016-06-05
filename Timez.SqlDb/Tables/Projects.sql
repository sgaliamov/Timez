CREATE TABLE [dbo].[Projects] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (50) NOT NULL,
    [BoardId] INT           NOT NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Projects_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id]) ON DELETE CASCADE
);

