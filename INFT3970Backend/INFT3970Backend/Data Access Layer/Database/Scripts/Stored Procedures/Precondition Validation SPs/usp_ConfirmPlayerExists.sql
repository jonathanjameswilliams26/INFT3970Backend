USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the PlayerID exists.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerExists
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_PLAYERDOESNOTEXIST INT = 12;

    IF NOT EXISTS (SELECT * FROM vw_All_Players WHERE PlayerID = @id)
	BEGIN
		SELECT @result = @EC_PLAYERDOESNOTEXIST;
		SET @errorMSG = 'The PlayerID does not exist';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO
