USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 23/09/18
-- Description:	Gets the current status of the game / application.
--				Used by the front end to get the current state of the application
--				when a user returns to the web application.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. EC_PLAYERNOTACTIVE - The playerID passed in is not active
--		2. EC_PLAYERDOESNOTEXIST - The playerID passed in does not exist

-- =============================================
CREATE PROCEDURE [dbo].[usp_GetGameStatus] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@gameState VARCHAR(255) OUTPUT,
	@hasVotesToComplete BIT OUTPUT,
	@hasNotifications BIT OUTPUT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	BEGIN TRY  
		--Confirm the playerID passed in exists
		EXEC [dbo].[usp_ConfirmPlayerExists] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Get the GameState of the game
		SELECT @gameState = GameState FROM tbl_Game WHERE GameID = @gameID

		--Setting the default values for the notification and votes
		SET @hasNotifications = 0;
		SET @hasVotesToComplete = 0;

		--The game is currently playing, check to see if the player has any new votes / notifications
		--If the game is in any other state they will not have any notifications / votes to complete
		IF(@gameState LIKE 'PLAYING')
		BEGIN

			--Check to see if the player has any votes they need to complete
			DECLARE @countVotes INT = 0;
			SELECT @countVotes = COUNT(*)
			FROM vw_Incomplete_Votes
			WHERE 
				PlayerID = @playerID
			IF(@countVotes > 0)
			BEGIN
				SET @hasVotesToComplete = 1;
			END
			
			--Check to see if the player has any new notifications
			DECLARE @countNotifs INT = 0;
			SELECT @countNotifs = COUNT(*)
			FROM vw_Unread_Notifications
			WHERE
				PlayerID = @playerID	
			IF(@countNotifs > 0)
			BEGIN
				SET @hasNotifications = 1;
			END
			
			--Get the player record
			SELECT * FROM vw_Join_PlayerGame WHERE PlayerID = @playerID
		END
		
		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH

	END CATCH
END
GO