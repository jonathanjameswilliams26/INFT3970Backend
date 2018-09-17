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
	@playerID VARCHAR(6),
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

		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		-- fetch gameID from player
		DECLARE @gameID INT;
		SELECT @gameID = gameID FROM tbl_Player WHERE PlayerID = @playerID

		-- set player HasLeftGame to true
		UPDATE tbl_Player SET HasLeftGame = 1, PlayerIsDeleted = 1 WHERE PlayerID = @playerID

		-- decrement number of players in respective game
		UPDATE tbl_Game SET NumOfPlayers = NumOfPlayers-1 WHERE GameID = @gameID

		-- delete photos submitted by player that are not voted upon yet
		UPDATE tbl_Photo SET PhotoIsDeleted = 1 WHERE TakenByPlayerID = @playerID AND IsVotingComplete = 0
	
		-- delete votes submitted by player that have not reached an outcome
		UPDATE tbl_PlayerVotePhoto SET PlayerVotePhotoIsDeleted = 1 WHERE PlayerID = @playerID AND PlayerVotePhotoIsActive = 0
	
		-- delete any unread notifications for the player
		UPDATE tbl_Notification SET NotificationIsDeleted = 1 WHERE PlayerID = @playerID

		--Set the success return variables
		SET @result = 1;
		SET @errorMSG = '';

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