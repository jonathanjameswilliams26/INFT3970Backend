USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the PlayerID is active. If the playerID active the procedure will return
--				Otherwise the procedure will raise an error.
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

	DECLARE @EC_PLAYERNOTACTIVE INT = 10;

    IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @id AND PlayerIsActive = 1 AND PlayerIsDeleted = 0)
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
GO