USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Adds a notification to the DB to then be used.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. The gameID does not exist
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateNotification] 
	-- Add the parameters for the stored procedure here
	@msgTxt VARCHAR(255),
	@type CHAR(8),
	@gameID INT,
	@playerID INT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRY  
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the new connectionID does not already exists
		IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @gameID)
		BEGIN
			SET @errorMSG = 'The gameID does not exist';
			RAISERROR('ERROR: gameID does not exist',16,1);
		END;


		--PlayerID exists and connectionID exists, add the notif
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			IF (@type = 'JOIN') -- if the type is of JOIN, then all players need to receive the notif
			BEGIN
				DECLARE @notifPlayerID INT
				DECLARE idCursor CURSOR FOR SELECT PlayerID FROM vw_PlayerGame WHERE GameID = @gameID --open a cursor for the resulting table
				OPEN idCursor

				FETCH NEXT FROM idCursor INTO @notifPlayerID
				WHILE @notifPlayerID != @playerID --@@FETCH_STATUS = 0 --iterate through all players and give them a notif
				BEGIN
					INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, IsActive, GameID, PlayerID) VALUES (@msgTxt, @type, 0, 1, @gameID, @notifPlayerID) -- insert into table with specific playerID
					FETCH NEXT FROM idCursor INTO @notifPlayerID  --iterate to next playerID
				END
				CLOSE idCursor -- close down cursor
				DEALLOCATE idCursor
			END
			ELSE    --else if notif is not of a type required to send to all, just create singular
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, IsActive, GameID, PlayerID) VALUES (@msgTxt, @type, 0, 1, @gameID, @playerID)

		COMMIT
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