USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 30/08/18
-- Description:	Decrements a players ammo count. When the player has taken a photo
--				their ammo is decremented and their number of photos taken is increased.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. @EC_DATAINVALID - The player's ammo is already at 0, cannot decrease ammo
--		2. @EC_INSERTERROR - An error occurred while trying to update the player record.

-- =============================================
CREATE PROCEDURE [dbo].[usp_UseAmmo] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_DATAINVALID INT = 17;
	DECLARE @EC_INSERTERROR INT = 2

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the Ammo Count is not already at 0
		DECLARE @ammoCount INT;
		SELECT @ammoCount = AmmoCount FROM tbl_Player WHERE PlayerID = @playerID
		IF(@ammoCount = 0)
		BEGIN
			SET @result = @EC_DATAINVALID;
			SET @errorMSG = 'The players ammo count is already at 0, cannot use ammo.';
			RAISERROR('', 16, 1);
		END

		--Make the update on the Players ammo count and number of photos taken
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET AmmoCount = AmmoCount - 1, NumPhotosTaken = NumPhotosTaken + 1
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