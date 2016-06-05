CREATE TABLE [dbo].[Organizations] (
    [Id]             INT                IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (500)     NOT NULL,
    [TariffId]       INT                NOT NULL,
    [IsFree]         BIT                CONSTRAINT [DF_Organizations_IsFree] DEFAULT ((0)) NOT NULL,
    [Css]            NVARCHAR (500)     NULL,
    [Logo]           NVARCHAR (500)     NULL,
    [Money]          MONEY              CONSTRAINT [DF_Organizations_Money] DEFAULT ((0)) NOT NULL,
    [WithdrawalDate] DATETIMEOFFSET (7) NULL,
    [IsBlocked]      BIT                CONSTRAINT [DF_Organizations_IsBlocked] DEFAULT ((0)) NOT NULL,
    [InviteCode]     VARCHAR (50)       CONSTRAINT [DF_Organizations_InviteCode] DEFAULT (newid()) NOT NULL,
    CONSTRAINT [PK_Organizations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Organizations_Tariffs] FOREIGN KEY ([TariffId]) REFERENCES [dbo].[Tariffs] ([Id])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Связть должна быть некаскадной, так как организации не должны удаляться вообще', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Organizations', @level2type = N'CONSTRAINT', @level2name = N'FK_Organizations_Tariffs';

