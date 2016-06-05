CREATE TABLE [dbo].[BoardsColors] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (50) NOT NULL,
    [Color]     NVARCHAR (50) NOT NULL,
    [BoardId]   INT           NOT NULL,
    [Position]  INT           NOT NULL,
    [IsDefault] BIT           CONSTRAINT [DF_BoardsColors_IdDefault] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Colors] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BoardsColors_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id])
);


GO
CREATE TRIGGER [dbo].[BoardsColorsInsertTrigger]
   ON  [dbo].[BoardsColors]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	
    -- Установка позиции
    UPDATE BoardsColors 
    SET [Position] = Id 
    WHERE Id IN (SELECT [Id] FROM Inserted);    
END

GO
CREATE TRIGGER [dbo].[BoardsColorsDeleteTrigger]
   ON  [dbo].[BoardsColors]
   INSTEAD OF DELETE
AS 
BEGIN SET NOCOUNT ON;

DECLARE @deletedId INT, @boardId INT, @IsDefault BIT;

DECLARE delCursor CURSOR FAST_FORWARD FOR SELECT [Id], IsDefault, BoardId FROM Deleted
OPEN delCursor
	FETCH NEXT FROM delCursor 
	INTO @deletedId, @IsDefault, @boardId;
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
--#region Deleting
		DECLARE @BoardIsDelete BIT;
		SELECT  @BoardIsDelete = IsDelete
		FROM    Boards
		WHERE	Id = @boardId

		IF @BoardIsDelete = 0
		BEGIN		
			-- Запрещаем удалять дефолтный цвет
			IF @isDefault = 1
			BEGIN
				RAISERROR('Запрещено удалять приоритет поумолчанию.', 13, 11)
 				SELECT CAST(N'Запрещено удалять приоритет поумолчанию.' AS INT);
				RETURN;
			END;	

			DECLARE	@Color NVARCHAR(50),
					@ColorPosition INT,
					@ColorName NVARCHAR(50),
					@defaultId INT = NULL;

			SELECT	@defaultId = Id
					, @Color= Color
					, @ColorPosition = [Position]
					, @ColorName = [Name]
			FROM	BoardsColors 
			WHERE	BoardId = @boardId AND IsDefault = 1

			IF @defaultId IS NULL
			BEGIN
				RAISERROR('На доске должен быть приоритет поумолчанию.', 13, 11);
				SELECT CAST(N'На доске должен быть приоритет поумолчанию.' AS INT)
				RETURN;
			END;

			-- назначаем дефолтный цвет задачам на доске, если удаляется не дефолтный цвет
			-- обновляем задачи
			UPDATE	Tasks
			SET		ColorId = @defaultId
					, ColorHEX = @Color
					, ColorPosition = @ColorPosition
					, ColorName = @ColorName
			WHERE	ColorId  = @deletedId;

			-- обновляем архив
			UPDATE	TasksArchive
			SET		ColorId = @defaultId
					, ColorHEX = @Color
					, ColorPosition = @ColorPosition
					, ColorName = @ColorName
			WHERE	ColorId  = @deletedId;
		END

		-- само удаление
		DELETE 
		FROM	BoardsColors 
		WHERE	[Id] = @deletedId;
--#endregion

		FETCH NEXT FROM delCursor 
		INTO @deletedId, @IsDefault, @boardId;
	END
CLOSE delCursor;
DEALLOCATE delCursor;
       
END
