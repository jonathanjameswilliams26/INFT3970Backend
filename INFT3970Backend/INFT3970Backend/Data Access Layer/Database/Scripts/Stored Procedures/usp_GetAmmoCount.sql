-- =============================================
-- Author:		Jonathan Williams
-- Create date: 18/09/18
-- Description:	Gets the player's ammo count
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetAmmoCount] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@ammoCount INT OUTPUT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @ammoCount = -1;

	--Confirm the PlayerID passed in exists and is active
	EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	SELECT @ammoCount = AmmoCount
	FROM tbl_Player
	WHERE PlayerID = @playerID

	SET @result = 1;
	SET @errorMSG = '';
END
GO