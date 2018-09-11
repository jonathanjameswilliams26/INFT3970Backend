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

	--Declaring the possible error codes returned
	DECLARE @EC_PLAYERIDDOESNOTEXIST INT = 12;

	--Confirm the playerID passed in exists
	IF NOT EXISTS (SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @playerID)
	BEGIN
		SET @result = @EC_PLAYERIDDOESNOTEXIST;
		SET @errorMSG = 'The playerID does not exist';
		RAISERROR('',16,1);
	END

	SELECT *
	FROM vw_PlayerVoteJoinTables
	WHERE PlayerID = @playerID AND IsPhotoSuccessful IS NULL
	ORDER BY VoteID

	SET @result = 1;
	SET @errorMSG = '';
	
END
GO