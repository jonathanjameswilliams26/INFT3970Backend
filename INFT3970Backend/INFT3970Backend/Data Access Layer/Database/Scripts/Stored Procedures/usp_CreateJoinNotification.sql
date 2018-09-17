USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Adds a Join notification to each player in the game.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. EC_PLAYERNOTACTIVE - The playerID passed in is not active
--		2. EC_PLAYERDOESNOTEXIST - The playerID passed in does not exist
--		3. EC_INSERTERROR - When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateJoinNotification] 
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

		--Confirm the game is not completed
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Add the join notification to each other player in the game.
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION

			--Get the message which will be added as the notification text
			DECLARE @msgTxt VARCHAR(255)
			SELECT @msgTxt = p.Nickname + ' has joined the game.' FROM tbl_Player p WHERE p.PlayerID = @PlayerID


			--Create the notifications for all other players in the game
			INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
			SELECT @msgTxt, 'JOIN', 0, 1, @gameID, PlayerID
			FROM vw_ActiveAndVerifiedPlayers
			WHERE PlayerID <> @playerID AND GameID = @gameID

		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to create the join notification.'
		END
	END CATCH
END
GO