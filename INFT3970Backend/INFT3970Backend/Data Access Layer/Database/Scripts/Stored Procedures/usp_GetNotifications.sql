USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Returns all the notifications for a player

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_GetNotifications] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@all BIT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_PLAYERIDDOESNOTEXIST INT = 12;

	BEGIN TRY
		
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('',16,1);
		END

		--The playerID exists, get the notifications associated with that player
		IF (@all = 1)
		SELECT * FROM tbl_Notification WHERE PlayerID = @playerID	
		ELSE
		SELECT * FROM tbl_Notification WHERE PlayerID = @playerID AND IsRead = 0

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH
		
	END CATCH
END
GO