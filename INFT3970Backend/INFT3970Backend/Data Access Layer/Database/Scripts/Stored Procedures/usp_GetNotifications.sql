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
	BEGIN TRY
		--Validate the playerID
		EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the Game is PLAYING state
		EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @gameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--The playerID exists, get the notifications associated with that player
		IF (@all = 1)
			SELECT * FROM vw_Active_Notifications WHERE PlayerID = @playerID	
		ELSE
			SELECT * FROM vw_Unread_Notifications WHERE PlayerID = @playerID

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH
		
	END CATCH
END
GO