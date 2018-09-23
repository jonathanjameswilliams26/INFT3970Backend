USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the PlayerID exists and is active.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--The error code returned if the player is not active
	DECLARE @EC_PLAYERNOTACTIVE INT = 10;

	--Confirm the playerID passed in exists
	EXEC [dbo].[usp_ConfirmPlayerExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT

	--If the player exists, check if the player is active
	IF(@result = 1)
	BEGIN
		IF NOT EXISTS (SELECT * FROM vw_Active_Players WHERE PlayerID = @id)
		BEGIN
			SELECT @result = @EC_PLAYERNOTACTIVE;
			SET @errorMSG = 'The PlayerID is not active.';
		END
		ELSE
		BEGIN
			SELECT @result = 1;
			SET @errorMSG = '';
		END
	END
END
GO