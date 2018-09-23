USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 11/09/18
-- Description:	Updates a PlayerVotePhoto record with a players vote decision

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The PlayerVotePhoto record does not exist with the specified VoteID and PlayerID
--		2. The Photo record voting is already completed
--		3. The photo voting finish time has already passed

-- =============================================
CREATE PROCEDURE [dbo].[usp_VoteOnPhoto] 
	-- Add the parameters for the stored procedure here 
	@voteID INT,
	@playerID INT,
	@isPhotoSuccessful BIT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_VOTEPHOTO_VOTERECORDDOESNOTEXIST INT = 4000;
	DECLARE @EC_VOTEPHOTO_VOTEALREADYCOMPLETE INT = 4001;
	DECLARE @EC_VOTEPHOTO_VOTEFINISHTIMEPASSED INT = 4002;

	BEGIN TRY
		
		--Validate the playerID
		EXEC [dbo].[usp_ConfirmPlayerInGame] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the Game is PLAYING state
		EXEC [dbo].[usp_ConfirmGameStateCorrect] @gameID = @gameID, @correctGameState = 'PLAYING', @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the vote record exists and has not already been voted on.
		IF NOT EXISTS (SELECT * FROM vw_Incomplete_Votes WHERE VoteID = @voteID)
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTERECORDDOESNOTEXIST;
			SET @errorMSG = 'The PlayerVotePhoto record does not exist or already voted.';
			RAISERROR('',16,1);
		END

		--Get the photoID from the Vote record
		DECLARE @photoID INT;
		SELECT @photoID = PhotoID FROM vw_Incomplete_Votes WHERE VoteID = @voteID

		--Confirm the voting is not already complete on the photo record
		IF EXISTS (SELECT * FROM vw_Completed_Photos WHERE PhotoID = @photoID)
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTEALREADYCOMPLETE;
			SET @errorMSG = 'The Voting on the photo has already been completed.';
			RAISERROR('',16,1);
		END

		--Confirm the Voting Finish Time has not already passed
		DECLARE @votingFinishTime DATETIME2;
		SELECT @votingFinishTime = VotingFinishTime FROM tbl_Photo WHERE PhotoID = @photoID
		IF(@votingFinishTime < GETDATE())
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTEFINISHTIMEPASSED;
			SET @errorMSG = 'The voting finish time has already passed.';
			RAISERROR('',16,1);
		END



		--If reaching this point all pre-condition checks have passed successfully


		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			
			--Update the Vote record with the decision
			UPDATE tbl_Vote
			SET IsPhotoSuccessful = @isPhotoSuccessful
			WHERE VoteID = @voteID

			--Update the photo record with the number of Yes or No Votes
			DECLARE @countYes INT = 0;
			DECLARE @countNo INT = 0;
			SELECT @countYes = COUNT(*) FROM vw_Success_Votes WHERE PhotoID = @photoID
			SELECT @countNo = COUNT(*) FROM vw_Fail_Votes WHERE PhotoID = @photoID
			UPDATE tbl_Photo
			SET NumYesVotes = @countYes, NumNoVotes = @countNo
			WHERE PhotoID = @photoID


			--Update the photo's IsVotingComplete field if all the player votes have successfully been completed
			IF NOT EXISTS (SELECT * FROM vw_Incomplete_Votes WHERE PhotoID = @photoID)
			BEGIN
				UPDATE tbl_Photo
				SET IsVotingComplete = 1
				WHERE PhotoID = @photoID

				-- if successful vote
				IF (@countYes > @countNo)
				BEGIN
					-- updating kills and deaths per players in the photo
					UPDATE tbl_Player 
					SET NumKills = NumKills +1 
					WHERE PlayerID = 
						(SELECT TakenByPlayerID
						FROM tbl_Photo
						WHERE PhotoID = @photoID)

					UPDATE tbl_Player 
					SET NumDeaths = NumDeaths +1 
					WHERE PlayerID = 
						(SELECT PhotoOfPlayerID 
						FROM tbl_Photo
						WHERE PhotoID = @photoID)
				END
			END
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_Join_VotePhotoPlayer WHERE VoteID = @voteID
	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to cast your vote.'
		END
	END CATCH
END
GO