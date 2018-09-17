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
CREATE PROCEDURE usp_ConfirmGameExists
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_GAMEDOESNOTEXIST INT = 13;

    IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @id AND GameIsDeleted = 0)
	BEGIN
		SELECT @result = @EC_GAMEDOESNOTEXIST;
		SET @errorMSG = 'The Game does not exist.';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO