-- =============================================
-- Author:		Jonathan Williams
-- Create date: 10/09/18
-- Description:	Gets the PlayerVotePhoto records which the PlayerID must completed / has not voted on yet
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetVotesToComplete] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Validate the playerID
	EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result
		
	--Get the GameID from the playerID
	DECLARE @gameID INT;
	EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

	--Confirm the Game is PLAYING state
	EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @gameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	--Gets the votes to complete by the player.
	SELECT *
	FROM 
		vw_Join_VotePhotoPlayer
	WHERE
		PlayerID = @playerID AND 
		IsPhotoSuccessful IS NULL AND 
		VoteIsActive = 1 AND 
		VoteIsDeleted = 0
	ORDER BY 
		VoteID

	SET @result = 1;
	SET @errorMSG = '';
	
END
GO