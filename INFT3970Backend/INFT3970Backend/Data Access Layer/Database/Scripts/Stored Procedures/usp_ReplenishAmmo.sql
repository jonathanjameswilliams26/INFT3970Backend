USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 30/08/18
-- Description:	Replenishes a players ammo by one.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. @EC_INSERTERROR - An error occurred while trying to update the player record.

-- =============================================
CREATE PROCEDURE [dbo].[usp_ReplenishAmmo] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_INSERTERROR INT = 2

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Make the update on the Players ammo count
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET AmmoCount = AmmoCount + 1
			WHERE PlayerID = @playerID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @playerID
	END TRY


	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to update the player record.'
		END
	END CATCH
END
GO