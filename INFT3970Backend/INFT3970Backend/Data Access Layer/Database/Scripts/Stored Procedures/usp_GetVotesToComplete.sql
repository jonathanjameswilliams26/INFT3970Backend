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

	--Confirm the playerID passed in exists and is active
	EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	SELECT *
	FROM vw_PlayerVoteJoinTables
	WHERE PlayerID = @playerID AND IsPhotoSuccessful IS NULL AND PlayerVotePhotoIsActive = 1 AND PlayerVotePhotoIsDeleted = 0
	ORDER BY VoteID

	SET @result = 1;
	SET @errorMSG = '';
	
END
GO