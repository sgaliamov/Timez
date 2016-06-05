CREATE PROCEDURE [dbo].[ReassignTasks]
	@boardId INT,
	@fromUserId INT,
	@toUserId INT
AS
SET NOCOUNT ON

BEGIN TRAN

	-- обновляем создателя
	UPDATE	Tasks
	SET		CreatorUserId = @toUserId
	WHERE	CreatorUserId = @fromUserId
	AND		BoardId = @boardId

	-- Инфа об исполнителе
	DECLARE	@ExecutorNick NVARCHAR(100), @ExecutorEmail NVARCHAR(320)
	SELECT	@ExecutorNick = u.Nick,
			@ExecutorEmail = u.EMail
	FROM	Users u
	WHERE	u.Id = @toUserId

	-- обновляем исполнителя
	UPDATE	Tasks
	SET		ExecutorUserId = @toUserId,
			ExecutorNick = @ExecutorNick,
			ExecutorEmail = @ExecutorEmail
	WHERE	ExecutorUserId = @fromUserId
	AND		BoardId = @boardId

COMMIT
