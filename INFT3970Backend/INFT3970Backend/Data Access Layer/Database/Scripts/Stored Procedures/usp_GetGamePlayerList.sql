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
--		1. The playerID trying to update does not exist

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
		
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('',16,1);
		END

		--The playerID exists, get the GameID associated with that player
		DECLARE @gameID INT;
		SELECT @gameID = GameID FROM tbl_Player WHERE PlayerID = @playerID;

		--Get all the active and verified players inside that game
		SELECT *
		FROM vw_PlayerGame
		WHERE GameID = @gameID AND PlayerIsActive = 1 AND isVerified = 1

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH

	END CATCH

END
GO