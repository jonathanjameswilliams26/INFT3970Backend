USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 14/09/18
-- Description:	Marks player notifications as read.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record

-- =============================================

-- create type to store incoming data into a table
DROP TYPE IF EXISTS udtNotifs
GO
CREATE TYPE udt_Notifs AS TABLE
    ( 
        notificationID VARCHAR(6)
    )
GO

CREATE PROCEDURE [dbo].[usp_SetNotificationsRead] 
	-- Add the parameters for the stored procedure here
	@udtNotifs AS dbo.udt_Notifs READONLY,
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

	DECLARE @notifID INT;

	BEGIN TRY

		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		DECLARE idCursor CURSOR FOR SELECT notificationID FROM @udtNotifs --open a cursor from the data table
		OPEN idCursor

		FETCH NEXT FROM idCursor INTO @notifID
		WHILE @@FETCH_STATUS = 0 --iterate through the notification IDs in the table
		BEGIN
			UPDATE tbl_Notification SET IsRead = 1 WHERE PlayerID = @playerID AND NotificationID = @notifID-- update IsRead status on the notification							
			FETCH NEXT FROM idCursor INTO @notifID  --iterate to next notifID
		END
		CLOSE idCursor -- close down cursor
		DEALLOCATE idCursor		


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