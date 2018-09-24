

USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 13/09/18
-- Description:	Removes a player to a game.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record

-- =============================================
CREATE PROCEDURE [dbo].[usp_LeaveGame] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@isGameCompleted BIT OUTPUT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY
		
		SET @isGameCompleted = 0;

		--Validate the playerID
		EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result


		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the Game is not completed
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION

			--Update the player record
			UPDATE tbl_Player
			SET HasLeftGame = 1
			WHERE PlayerID = @playerID

			--Update the number of players in the game
			UPDATE tbl_Game
			SET NumOfPlayers = NumOfPlayers - 1
			WHERE GameID = @gameID

			--If the current game state is in lobby leave the stored procedure because there is nothing else to perform.
			IF(SELECT GameState From vw_Active_Games WHERE GameID = @gameID) LIKE 'IN LOBBY'
			BEGIN
				SET @result = 1;
				SET @errorMSG = '';
				COMMIT;
				RETURN;
			END

			--Check to see if the game is completed because it has reached the minimum number of players
			DECLARE @countPlayers INT;
			SELECT @countPlayers = COUNT(*) FROM vw_InGame_Players WHERE GameID = @gameID
			IF(@countPlayers < 3)
			BEGIN
				--Call the end game stored procedure to end the game
				EXEC [dbo].[usp_CompleteGame] @gameID = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
				EXEC [dbo].[usp_DoRaiseError] @result = @result

				SET @isGameCompleted = 1;
				SET @result = 1;
				SET @errorMSG = '';
				COMMIT;
				--Leave the stored procedure because the game has now ended and there is nothing else to complete
				RETURN;
			END


			--If the current game state is STARTING leave the stored procedure because there is nothing else to perform.
			IF(SELECT GameState From vw_Active_Games WHERE GameID = @gameID) LIKE 'STARTING'
			BEGIN
				SET @result = 1;
				SET @errorMSG = '';
				COMMIT;
				RETURN;
			END


			--Delete all notifications received by the player
			UPDATE tbl_Notification
			SET NotificationIsDeleted = 1
			WHERE PlayerID = @playerID

			--Delete all votes on non completed photos submitted by the player.
			UPDATE tbl_Vote
			SET VoteIsDeleted = 1
			WHERE PhotoID IN (SELECT PhotoID FROM vw_Incompleted_Photos WHERE TakenByPlayerID = @playerID)

			--Delete all non completed photos submitted by the player
			UPDATE tbl_Photo
			SET PhotoIsDeleted = 1
			WHERE PhotoID IN (SELECT PhotoID FROM vw_Incompleted_Photos WHERE TakenByPlayerID = @playerID)

			--Create a table variable to store the PhotoID's being updated
			DECLARE @photosBeingUpdated TABLE ( id INT NOT NULL);

			--Insert into the the table variable, the ID's of the photos that are being updated / affected by the player leaving
			INSERT INTO @photosBeingUpdated (id)
			SELECT PhotoID
			FROM vw_Incomplete_Votes
			WHERE PlayerID = @playerID

			--Delete all incomplete votes made the player
			UPDATE tbl_Vote
			SET VoteIsDeleted = 1
			WHERE VoteID IN (SELECT VoteID FROM vw_Incomplete_Votes WHERE PlayerID = @playerID)

			--Loop through each of the photos and check to see if any of them have now been completed
			DECLARE @tempCursorID INT;
			DECLARE photoCursor CURSOR FOR     
			SELECT id
			FROM @photosBeingUpdated
			OPEN photoCursor
			FETCH NEXT FROM photoCursor
			INTO @tempCursorID

			--Loop through each photoID and check to see if the photo is now completed
			WHILE @@FETCH_STATUS = 0    
			BEGIN    
				--Call a procedure to update the number of yes/no votes and check to see
				--if the photo voting has now been completed. If completed update the kills/deaths
				EXEC [dbo].[usp_UpdateVotingCountOnPhoto] @photoID = @tempCursorID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
				EXEC [dbo].[usp_DoRaiseError] @result = @result

				FETCH NEXT FROM photoCursor
				INTO @tempCursorID
			END     
			CLOSE photoCursor;    
			DEALLOCATE photoCursor; 
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

		--Select any photos which have now been completed after the player left the game
		SELECT *
		FROM vw_Join_PhotoGamePlayers
		WHERE 
			PhotoID IN (SELECT id FROM @photosBeingUpdated) AND
			IsVotingComplete = 1 AND
			PhotoIsActive = 1 AND
			PhotoIsDeleted = 0
	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to remove the player from the game'
		END
	END CATCH
END
GO