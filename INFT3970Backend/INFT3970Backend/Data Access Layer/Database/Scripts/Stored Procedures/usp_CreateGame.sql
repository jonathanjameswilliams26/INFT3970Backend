USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Creates a new game in the database.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. EC_ITEMALREADYEXISTS - An active game already exists with that game code.

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateGame] 
	-- Add the parameters for the stored procedure here
	@gameCode VARCHAR(6),
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_ITEMALREADYEXISTS INT = 14;


	BEGIN TRY  
		--Confirm the game code does not already exist in an active game / currently playing game
		IF EXISTS (SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE GameCode = @gameCode)
		BEGIN
			SET @result = @EC_ITEMALREADYEXISTS;
			SET @errorMSG = 'The game code already exists.';
			RAISERROR('',16,1);
		END;

		--Game code does not exist, create the new game
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			INSERT INTO tbl_Game (GameCode) VALUES (@gameCode);
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'The an error occurred while trying to create the game record';
		END

	END CATCH
END
GO