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
--				Will run normally if the playerID is active and exists
--				Will raise an error and return the error message and result if fails.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerExistsAndIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Call the procedure to confirm the playerID exists
    EXEC [dbo].[usp_ConfirmPlayerExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT

	IF(@result = 1)
	BEGIN
		--Call the proceduer to confirm the PlayerID is active
		EXEC [dbo].[usp_ConfirmPlayerIsActive] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	END
END
GO