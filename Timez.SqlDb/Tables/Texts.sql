CREATE TABLE [dbo].[Texts] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [Title]            NVARCHAR (MAX)     NOT NULL,
    [Content]          NVARCHAR (MAX)     NOT NULL,
    [TypeId]           INT                CONSTRAINT [DF_Texts_TypeId] DEFAULT ((0)) NOT NULL,
    [CreationDateTime] DATETIMEOFFSET (7) CONSTRAINT [DF_Texts_CreationDateTime] DEFAULT (getdate()) NOT NULL,
    [IsVisible]        BIT                CONSTRAINT [DF_Texts_IsVisible] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Texts] PRIMARY KEY CLUSTERED ([Id] ASC)
);

