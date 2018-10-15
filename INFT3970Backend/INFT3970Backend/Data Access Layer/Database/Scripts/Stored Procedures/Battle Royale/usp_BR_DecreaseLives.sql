USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 30/10/18
-- Description:	Decreases the number of lives the player has after
-- attempting to take a photo outside of the battle royale playing zone.

-- Returns: 1 = Successful, or 0 = An error occurred
-- =============================================
CREATE PROCEDURE [dbo].[usp_BR_DecreaseLives] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declare the error codes
	DECLARE @DATABASE_CONNECT_ERROR INT = 0;
    DECLARE @INSERT_ERROR INT = 2;
    DECLARE @BUILD_MODEL_ERROR INT = 3;
    DECLARE @ITEM_ALREADY_EXISTS INT = 4;
    DECLARE @DATA_INVALID INT = 5;
    DECLARE @ITEM_DOES_NOT_EXIST INT = 6;
    DECLARE @CANNOT_PERFORM_ACTION INT = 7;
    DECLARE @GAME_DOES_NOT_EXIST INT = 8;
    DECLARE @GAME_STATE_INVALID INT = 9;
    DECLARE @PLAYER_DOES_NOT_EXIST INT = 10;
    DECLARE @PLAYER_INVALID INT = 11;
    DECLARE @MODELINVALID_PLAYER INT = 12;
    DECLARE @MODELINVALID_GAME INT = 13;
    DECLARE @MODELINVALID_PHOTO INT = 14;
    DECLARE @MODELINVALID_VOTE INT = 15;

	BEGIN TRY  
		--Validate the playerID
		EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the Game is PLAYING state
		EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @gameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the lives is not already at 0
		DECLARE @lives INT;
		SELECT @lives = LivesRemaining FROM tbl_Player WHERE PlayerID = @playerID
		IF(@lives = 0)
		BEGIN
			SET @result = @CANNOT_PERFORM_ACTION;
			SET @errorMSG = 'The players lives is already at 0.';
			RAISERROR('', 16, 1);
		END

		--Make the update on the Players ammo count and number of photos taken
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET LivesRemaining = LivesRemaining - 1
			WHERE PlayerID = @playerID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_Join_PlayerGame WHERE PlayerID = @playerID
	END TRY


	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @INSERT_ERROR;
			SET @errorMSG = 'An error occurred while trying to update the player record.'
		END
	END CATCH
END
GO