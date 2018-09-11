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
	@gameID INT,
	@takenByID INT,
	@photoOfID INT,
	@result BIT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @msgTxt VARCHAR(255)
	DECLARE @notifPlayerID INT 

	BEGIN TRY  
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @photoOfID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @takenByID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the gameID exists
		IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @gameID)
		BEGIN
			SET @errorMSG = 'The gameID does not exist';
			RAISERROR('ERROR: gameID does not exist',16,1);
		END;

		IF (@result = 1) --success
		BEGIN
			SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
			BEGIN TRANSACTION

				-- send to the tagged player
				SET @msgTxt = 'You have been tagged by '
				SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @takenByID
				SET @msgTxt += '.'

				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @gameID, @photoOfID) -- insert into table with specific playerID			
						
				-- send to the tagging player
				SET @msgTxt = 'You successfully tagged '
				SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				SET @msgTxt += '.'

				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @gameID, @takenByID) -- insert into table with specific playerID	
						
				-- send to everyone else						
				SELECT @msgTxt = p.Nickname FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				SET @msgTxt += ' was tagged by '
				SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @takenByID
				SET @msgTxt += '.'

				DECLARE idCursor CURSOR FOR SELECT PlayerID FROM vw_PlayerGame WHERE GameID = @gameID --open a cursor for the resulting table
				OPEN idCursor

				FETCH NEXT FROM idCursor INTO @notifPlayerID
				WHILE @@FETCH_STATUS = 0 --iterate through all players and give them a notif
				BEGIN
					IF (@notifPlayerID != @photoOfID AND @notifPlayerID != @takenByID)
					BEGIN
						INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @gameID, @notifPlayerID) -- insert into table with specific playerID								
					END
					FETCH NEXT FROM idCursor INTO @notifPlayerID  --iterate to next playerID
				END
				CLOSE idCursor -- close down cursor
				DEALLOCATE idCursor							
			COMMIT
		END
		ELSE --fail
		BEGIN
		BEGIN
			SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
			BEGIN TRANSACTION

				-- send to the tagged player				
				SET @msgTxt = 'You were missed by '
				SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @takenByID
				SET @msgTxt += '.'

				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @gameID, @photoOfID) -- insert into table with specific playerID			
						
				-- send to the tagging player
				SET @msgTxt = 'You failed to tag '
				SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				SET @msgTxt += '.'

				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @gameID, @takenByID) -- insert into table with specific playerID	
						
				-- send to everyone else		
				SELECT @msgTxt = p.Nickname FROM tbl_Player p WHERE p.PlayerID = @takenByID
				SET @msgTxt += ' failed to tag '
				SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				SET @msgTxt += '.'

				DECLARE idCursor CURSOR FOR SELECT PlayerID FROM vw_PlayerGame WHERE GameID = @gameID --open a cursor for the resulting table
				OPEN idCursor

				FETCH NEXT FROM idCursor INTO @notifPlayerID
				WHILE @@FETCH_STATUS = 0 --iterate through all players and give them a notif
				BEGIN
					IF (@notifPlayerID != @photoOfID AND @notifPlayerID != @takenByID)
					BEGIN
						INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @gameID, @notifPlayerID) -- insert into table with specific playerID								
					END
					FETCH NEXT FROM idCursor INTO @notifPlayerID  --iterate to next playerID
				END
				CLOSE idCursor -- close down cursor
				DEALLOCATE idCursor							
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
		END

	END CATCH
END
GO