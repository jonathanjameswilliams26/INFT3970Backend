USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirm the GameState of the GameID passed in is as expected.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ConfirmGameStateCorrect]
	@gameID INT,
	@correctGameState VARCHAR(255),
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_GAMESTATEINVALID INT = 18;

	--Confirm the game exists and is active
	EXEC [dbo].[usp_ConfirmGameExistsAndIsActive] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	
	IF(@result = 1)
	BEGIN
		--Get the actual current game state and confirm the game state is correct
		DECLARE @actualGameState VARCHAR(255);
		SELECT @actualGameState = GameState FROM tbl_Game WHERE GameID = @gameID

		--Confirm the game state matches the game state passed in
		IF(@correctGameState NOT LIKE @actualGameState)
		BEGIN
			SET @result = @EC_GAMESTATEINVALID;
			SET @errorMSG = 'The game state is invalid for this operation. Expecting game state to be ' + @correctGameState + ' but the actual game state is ' + @actualGameState;
		END
		ELSE
		BEGIN
			SET @result = 1;
			SET @errorMSG = '';
		END
	END
END
GO