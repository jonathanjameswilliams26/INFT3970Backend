USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 19/09/18
-- Description:	Gets the last photos uploaded in the game by each player if
--				applicable, using their last photo taken as their last known
--				location.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. @EC_BEGINGAME_NOTHOST - The playerID is not the host of the game
--		3. @EC_BEGINGAME_NOTINLOBBY - The game is not currently IN LOBBY
--		4. @EC_BEGINGAME_NOTENOUGHPLAYERS - Trying to start a game with less than 3 active and verified players.

-- =============================================
CREATE PROCEDURE [dbo].[usp_GetLastKnownLocations] 
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

	BEGIN TRY  
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the Game is in a PLAYING state
		EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @gameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Perform the select statement to get the last known locations
		SELECT * 
		FROM 
			vw_PhotoGameAndPlayers p
		WHERE
			p.GameID = @gameID AND
			p.PhotoIsActive = 1 AND
			p.PhotoIsDeleted = 0 AND
			p.TimeTaken = (
				SELECT MAX(ph.TimeTaken)
				FROM tbl_Photo ph
				WHERE 
					ph.TakenByPlayerID = p.TakenByPlayerID AND 
					ph.GameID = @gameID AND
					ph.PhotoIsActive = 1 AND
					ph.PhotoIsDeleted = 0
			)
			AND TakenByPlayerID <> @playerID


		SET @result = 1
		SET @errorMSG = '';
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH

	END CATCH
END
GO







