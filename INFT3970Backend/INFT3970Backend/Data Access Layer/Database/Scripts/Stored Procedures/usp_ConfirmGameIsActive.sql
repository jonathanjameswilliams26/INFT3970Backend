USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the GameID is active. If the GameID active the procedure will return
--				Otherwise the procedure will raise an error.
-- =============================================
CREATE PROCEDURE usp_ConfirmGameIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_GAMENOTACTIVE INT = 9;

    IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @id AND GameIsActive = 1 AND GameIsDeleted = 0)
	BEGIN
		SELECT @result = @EC_GAMENOTACTIVE;
		SET @errorMSG = 'The Game is not active.';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO