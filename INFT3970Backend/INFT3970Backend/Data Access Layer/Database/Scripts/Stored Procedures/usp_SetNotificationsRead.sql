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
        notificationID INT
    )
GO

CREATE PROCEDURE [dbo].[usp_SetNotificationsRead] 
	-- Add the parameters for the stored procedure here
	@udtNotifs AS dbo.udt_Notifs READONLY,
	@playerID INT,
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

		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Update the notifications to IsRead = 1
		UPDATE tbl_Notification
		SET IsRead = 1
		WHERE PlayerID = @playerID AND NotificationID IN (
				SELECT notificationID
				FROM @udtNotifs
			)

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