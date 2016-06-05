CREATE TABLE [dbo].[Boards] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (50)  NOT NULL,
    [Description]    NVARCHAR (MAX) NULL,
    [RefreshPeriod]  INT            NULL,
    [OrganizationId] INT            NULL,
    [IsDelete]       BIT            CONSTRAINT [DF_Boards_IsDelete] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Boards] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Boards_Organizations] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organizations] ([Id])
);


GO
-- Запускается перед удалением доски
CREATE TRIGGER [dbo].[BoardsDeleteTrigger]
ON [dbo].[Boards]
INSTEAD OF DELETE
AS 
BEGIN
SET NOCOUNT ON;          	   

-- set board to deleting state
UPDATE	Boards SET IsDelete = 1 
WHERE Id IN (SELECT Id FROM Deleted)

-- into delete triggers in TasksStatuses and in BoardsColors the flag IsDelete is examened
DELETE TasksStatuses	WHERE BoardId	IN (SELECT Id FROM Deleted); -- статусы		
DELETE BoardsColors		WHERE BoardId	IN (SELECT Id FROM Deleted); -- цвета
DELETE Boards			WHERE [Id]		IN (SELECT Id FROM Deleted); -- доска
	
END;

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Находится ли доска в состоянии удаления, флаг используется в тригерах [BoardsColorsDeleteTrigger] и [TasksStatusDeleteTrigger]', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Boards', @level2type = N'COLUMN', @level2name = N'IsDelete';

