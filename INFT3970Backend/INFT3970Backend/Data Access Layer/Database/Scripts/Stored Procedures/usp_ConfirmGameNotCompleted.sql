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
--				If the game is not complete it will run normally,
--				otherwise, if the game is completed an error will be raised
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

	DECLARE @EC_GAMEALREADYCOMPLETE INT = 16;

	IF EXISTS (SELECT * FROM tbl_Game WHERE GameID = @id AND GameState = 'COMPLETED')
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
GO