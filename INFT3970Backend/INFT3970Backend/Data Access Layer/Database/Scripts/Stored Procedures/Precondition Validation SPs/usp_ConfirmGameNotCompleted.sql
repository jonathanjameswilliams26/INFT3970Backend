USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the Game is not already completed.
-- =============================================
CREATE PROCEDURE usp_ConfirmGameNotCompleted
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--The error code returned if the game is already completed
	DECLARE @EC_GAMEALREADYCOMPLETE INT = 16;

	--Confirm the game exists
	EXEC [dbo].[usp_ConfirmGameExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT

	--If the game exists check to see if the game is completed.
	IF(@result = 1)
	BEGIN
		IF NOT EXISTS (SELECT * FROM vw_Active_Games WHERE GameID = @id)
		BEGIN
			SELECT @result = @EC_GAMEALREADYCOMPLETE;
			SET @errorMSG = 'The game is already completed.';
		END
		ELSE
		BEGIN
			SELECT @result = 1;
			SET @errorMSG = '';
		END
	END
END
GO