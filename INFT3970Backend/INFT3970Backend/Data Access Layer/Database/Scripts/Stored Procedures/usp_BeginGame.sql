USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Begins the Game, sets the GameState to Starting and sets the StartTime and EndTime
--				Business Logic Code is scheduled to Run at start time to start the game and update the Game State to PLAYING.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. @EC_BEGINGAME_NOTHOST - The playerID is not the host of the game
--		3. @EC_BEGINGAME_NOTINLOBBY - The game is not currently IN LOBBY
--		4. @EC_BEGINGAME_NOTENOUGHPLAYERS - Trying to start a game with less than 3 active and verified players.

-- =============================================
ALTER PROCEDURE [dbo].[usp_BeginGame] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_BEGINGAME_NOTHOST INT = 6000;
	DECLARE @EC_BEGINGAME_NOTINLOBBY INT = 6001;
	DECLARE @EC_BEGINGAME_NOTENOUGHPLAYERS INT = 6002;

	BEGIN TRY  
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		

		--Confirm the playerID passed in Is the host of the Game because only the host can begin the game
		DECLARE @isHost BIT;
		SELECT @isHost = IsHost FROM tbl_Player WHERE PlayerID = @playerID
		IF(@isHost = 0)
		BEGIN
			SET @result = @EC_BEGINGAME_NOTHOST;
			SET @errorMSG = 'The playerID passed in is not the host player. Only the host can begin the game.';
			RAISERROR('',16,1);
		END

		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT


		--Confirm the Game is IN LOBBY state
		DECLARE @gameState VARCHAR(255);
		SELECT @gameState = GameState FROM tbl_Game WHERE GameID = @gameID
		IF(@gameState NOT LIKE 'IN LOBBY')
		BEGIN
			SET @result = @EC_BEGINGAME_NOTINLOBBY;
			SET @errorMSG = 'The game state is not IN LOBBY, the must be in the lobby to begin the game.';
			RAISERROR('',16,1);
		END


		--Confirm there is enough players in the Game to Start
		DECLARE @activePlayerCount INT = 0;
		SELECT @activePlayerCount = COUNT(*) FROM tbl_Player WHERE GameID = @gameID AND IsVerified = 1 AND PlayerIsActive = 1 AND PlayerIsDeleted = 0 AND HasLeftGame = 0
		IF(@activePlayerCount < 3)
		BEGIN
			SET @result = @EC_BEGINGAME_NOTENOUGHPLAYERS;
			SET @errorMSG = 'Not enough players to start the game, need a minimum of 3.';
			RAISERROR('',16,1);
		END


		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			--Update the GameState to STARTING, update the StartTime to 10mins in the future and update the EndTime to the endtime
			UPDATE tbl_Game
			SET GameState = 'STARTING', StartTime = DATEADD(MINUTE, 10, GETDATE()), EndTime = DATEADD(DAY, 1, GETDATE())
			WHERE GameID = @gameID
		COMMIT

		SET @result = 1
		SET @errorMSG = '';
		SELECT * FROM tbl_Game WHERE GameID = @gameID
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'The an error occurred while trying to create the game record';
		END
	END CATCH
END
GO