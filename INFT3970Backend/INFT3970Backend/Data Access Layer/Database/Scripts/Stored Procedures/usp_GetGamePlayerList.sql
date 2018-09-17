USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 1/09/18
-- Description:	Gets all the players in a game, takes in a playerID and users that playerID to find all other players in the game

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID passed in does not exist

-- =============================================
CREATE PROCEDURE [dbo].[usp_GetGamePlayerList] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
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
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Get the GameID from the player
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Get all players inside that game
		SELECT *
		FROM vw_PlayerGame
		WHERE GameID = @gameID

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH

	END CATCH

END
GO