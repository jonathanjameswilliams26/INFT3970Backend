USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Completes a game and sets all references to the game to not active.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. EC_GAMENOTEXISTS - The GameID passed in dows not exist.

-- =============================================
CREATE PROCEDURE [dbo].[usp_CompleteGame] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY  
		
		--Confirm the gameID passed in exists not complete
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the Game is PLAYING state
		EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @gameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Update the Game to now be completed.
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			--Update the game to completed
			UPDATE tbl_Game
			SET GameIsActive = 0, GameState = 'COMPLETED'
			WHERE GameID = @gameID

			--Update all the players in the game to not active
			UPDATE tbl_Player
			SET PlayerIsActive = 0
			WHERE GameID = @gameID

			--Update all photos in the game to not active
			UPDATE tbl_Photo
			SET PhotoIsActive = 0
			WHERE GameID = @gameID

			--Delete any photos which voting has not yet been completed
			UPDATE tbl_Photo
			SET PhotoIsDeleted = 1
			WHERE IsVotingComplete = 0 AND GameID = @gameID

			--Delete any votes which have not yet been completed in the game
			UPDATE tbl_Vote
			SET VoteIsDeleted = 1
			WHERE PhotoID IN (SELECT PhotoID FROM tbl_Photo WHERE PhotoIsDeleted = 1 AND GameID = @gameID)

			--Update all votes to not active
			UPDATE tbl_Vote
			SET VoteIsActive = 0
			WHERE PlayerID IN (SELECT PlayerID FROM tbl_Player WHERE GameID = @gameID)

			--Update all notifications to not active
			UPDATE tbl_Notification
			SET NotificationIsActive = 0
			WHERE GameID = @gameID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'The an error occurred while trying to complete the game.';
		END

	END CATCH
END
GO