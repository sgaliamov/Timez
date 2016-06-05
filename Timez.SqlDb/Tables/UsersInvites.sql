CREATE TABLE [dbo].[UsersInvites] (
    [UserId]         INT                NOT NULL,
    [EMail]          NVARCHAR (320)     NOT NULL,
    [InviteCode]     VARCHAR (50)       CONSTRAINT [DF_UsersInvites_InviteCode] DEFAULT (newid()) NOT NULL,
    [DateTime]       DATETIMEOFFSET (7) NOT NULL,
    [OrganizationId] INT                NOT NULL,
    CONSTRAINT [PK_UsersInvites] PRIMARY KEY CLUSTERED ([InviteCode] ASC),
    CONSTRAINT [FK_UsersInvites_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);

