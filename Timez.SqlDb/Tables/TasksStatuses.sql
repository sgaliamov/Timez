CREATE TABLE [dbo].[TasksStatuses] (
    [Id]                  INT           IDENTITY (1, 1) NOT NULL,
    [Name]                NVARCHAR (50) NOT NULL,
    [BoardId]             INT           NOT NULL,
    [Position]            INT           NOT NULL,
    [NeedTimeCounting]    BIT           CONSTRAINT [DF_TaskStatus_TimeCounting] DEFAULT ((0)) NOT NULL,
    [MaxTaskCountPerUser] INT           NULL,
    [IsBacklog]           BIT           CONSTRAINT [DF_TasksStatus_IsBacklog] DEFAULT ((0)) NOT NULL,
    [PlanningRequired]    BIT           CONSTRAINT [DF_TasksStatuses_PlanningRequired] DEFAULT ((0)) NOT NULL,
    [MaxPlanningTime]     INT           NULL,
    CONSTRAINT [PK_TaskStatus] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TasksStatus_Boards] FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_TasksStatuses_BoardId]
    ON [dbo].[TasksStatuses]([BoardId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TasksStatuses_IsBacklog]
    ON [dbo].[TasksStatuses]([IsBacklog] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TasksStatuses_Position]
    ON [dbo].[TasksStatuses]([Position] ASC);


GO
CREATE TRIGGER [dbo].[TasksStatusInsertTrigger]
   ON  [dbo].[TasksStatuses]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	
	UPDATE TasksStatuses 
	SET [Position] = Id 
	WHERE Id IN (SELECT [Id] FROM Inserted);	
END

GO
CREATE TRIGGER [dbo].[TasksStatusDeleteTrigger]
   ON  [dbo].[TasksStatuses] 
   INSTEAD OF DELETE
AS 
BEGIN SET NOCOUNT ON;          

DECLARE @deletedId INT, @boardId INT, @IsBacklog BIT;

DECLARE delCursor CURSOR FAST_FORWARD FOR SELECT [Id], IsBacklog, BoardId FROM Deleted
OPEN delCursor
	FETCH NEXT FROM delCursor 
	INTO @deletedId, @IsBacklog, @boardId;
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
--#region Deleting
DECLARE	@BoardIsDelete BIT;
SELECT  @BoardIsDelete = IsDelete
FROM    Boards
WHERE	Id = @boardId

IF @BoardIsDelete = 0
BEGIN
	-- при единичном удалении заменяем статус на беклог
	IF (@IsBacklog = 1)
	BEGIN
		RAISERROR('Запрещено удалять беклог.', 13, 11)
 		SELECT CAST(N'Запрещено удалять беклог.' AS INT);
		RETURN;
	END

	DECLARE	@BacklogId INT = NULL, @TaskStatusPosition INT, @TaskStatusName NVARCHAR(50);
	SELECT	@BacklogId = Id
			, @TaskStatusPosition = [Position]
			, @TaskStatusName = [Name]
	FROM	[TasksStatuses]
	WHERE	BoardId = @boardId 
	AND		IsBacklog = 1

	IF @BacklogId IS NULL
	BEGIN
		RAISERROR('Беклог не найден.', 13, 11);
		SELECT CAST(N'Беклог не найден.' AS INT)
		RETURN;
	END;

	-- назначаем дефолтный цвет задачам на доске, если удаляется не дефолтный цвет
	-- обновляем задачи
	UPDATE	Tasks
	SET		TaskStatusId = @BacklogId
			, TaskStatusPosition = @TaskStatusPosition
			, TaskStatusName = @TaskStatusName
	WHERE	TaskStatusId  = @deletedId;
END

-- само удаление
DELETE 
FROM	[TasksStatuses] 
WHERE	[Id] = @deletedId;
--#endregion

		FETCH NEXT FROM delCursor 
		INTO @deletedId, @IsBacklog, @boardId;
	END
CLOSE delCursor;
DEALLOCATE delCursor;

END
