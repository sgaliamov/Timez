CREATE TABLE [dbo].[OrganizationUsers] (
    [Id]             INT IDENTITY (1, 1) NOT NULL,
    [UserId]         INT NOT NULL,
    [OrganizationId] INT NOT NULL,
    [IsApproved]     BIT CONSTRAINT [DF_OrganizationUsers_IsApproved] DEFAULT ((0)) NOT NULL,
    [UserRole]       INT NOT NULL,
    CONSTRAINT [PK_OrganizationUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OrganizationUsers_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrganizationUsers_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);

