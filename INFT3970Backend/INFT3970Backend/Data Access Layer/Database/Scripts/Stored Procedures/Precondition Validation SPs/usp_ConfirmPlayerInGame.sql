USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 23/09/18
-- Description:	Confirms the PlayerID passed in has not left the game.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerInGame
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_PLAYERNOTINGAME INT = 8;

	--Confirm the PlayerID exists and isActive
	EXEC [dbo].[usp_ConfirmPlayerIsActive] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT

	--If the player exists and isactive, check to see if the player is in the game.
	IF(@result = 1)
	BEGIN
		IF NOT EXISTS (SELECT * FROM vw_InGame_Players WHERE PlayerID = @id)
		BEGIN
			SELECT @result = @EC_PLAYERNOTINGAME;
			SET @errorMSG = 'The PlayerID is not in the game.';
		END
		ELSE
		BEGIN
			SELECT @result = 1;
			SET @errorMSG = '';
		END
	END
END
GO