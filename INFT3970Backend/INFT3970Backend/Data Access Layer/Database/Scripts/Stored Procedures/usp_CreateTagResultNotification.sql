USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Creates a tag notification for each player depending on the result of a tag.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. The gameID does not exist
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateTagResultNotification] 
	-- Add the parameters for the stored procedure here
	@takenByID INT,
	@photoOfID INT,
	@decision BIT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @msgTxt VARCHAR(255)
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_DATAINVALID INT = 17;

	BEGIN TRY  
		--Validate the takenByID
		EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @takenByID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Validate the photoOfByID
		EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @photoOfID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Get the GameID of the TakenByPLayerID and PhotoOfPlayerID and confirm they are in the same game
		DECLARE @takenByGameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @takenByID, @gameID = @takenByGameID OUTPUT
		DECLARE @photoOfGameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @photoOfID, @gameID = @photoOfGameID OUTPUT
		IF(@takenByGameID <> @photoOfGameID)
		BEGIN
			SET @result = @EC_DATAINVALID;
			SET @errorMSG = 'The players provided are not in the same game.'
		END

		--Confirm the Game is PLAYING state
		EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @takenByGameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		IF (@decision = 1) --success
		BEGIN
			SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
			BEGIN TRANSACTION

				-- send to the tagged player
				SELECT @msgTxt = 'You have been tagged by ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @takenByID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @takenByGameID, @photoOfID) -- insert into table with specific playerID			
						
				-- send to the tagging player
				SELECT @msgTxt = 'You successfully tagged ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @takenByGameID, @takenByID) -- insert into table with specific playerID	
						
				-- send to everyone else						
				SELECT @msgTxt = p.Nickname + ' was tagged by ' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				SELECT @msgTxt += p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @takenByID

				--Create the notifications for all other players in the game
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
				SELECT @msgTxt, 'SUCCESS', 0, 1, @takenByGameID, PlayerID
				FROM vw_InGame_Players
				WHERE PlayerID <> @takenByID AND PlayerID <> @photoOfID AND GameID = @takenByGameID						
			COMMIT
		END
		ELSE --fail
		BEGIN
		BEGIN
			SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
			BEGIN TRANSACTION

				-- send to the tagged player				
				SELECT @msgTxt = 'You were missed by ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @takenByID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @takenByGameID, @photoOfID) -- insert into table with specific playerID			
						
				-- send to the tagging player
				SELECT @msgTxt = 'You failed to tag ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @takenByGameID, @takenByID) -- insert into table with specific playerID	
						
				-- send to everyone else		
				SELECT @msgTxt = p.Nickname + ' failed to tag ' FROM tbl_Player p WHERE p.PlayerID = @takenByID
				SELECT @msgTxt += p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				
				--Create the notifications for all other players in the game
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
				SELECT @msgTxt, 'FAIL', 0, 1, @takenByGameID, PlayerID
				FROM vw_InGame_Players
				WHERE PlayerID <> @takenByID AND PlayerID <> @photoOfID AND GameID = @takenByGameID						
			COMMIT
		END
		END	
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