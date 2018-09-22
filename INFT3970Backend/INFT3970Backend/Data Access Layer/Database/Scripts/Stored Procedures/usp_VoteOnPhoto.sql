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
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the vote record exists
		IF NOT EXISTS (SELECT * FROM tbl_PlayerVotePhoto WHERE PlayerID = @playerID AND VoteID = @voteID AND PlayerVotePhotoIsActive = 1 AND IsPhotoSuccessful IS NULL AND PlayerVotePhotoIsDeleted = 0)
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTERECORDDOESNOTEXIST;
			SET @errorMSG = 'The PlayerVotePhoto record does not exist.';
			RAISERROR('',16,1);
		END

		--Get the photoID from the PlayerVotePhoto record
		DECLARE @photoID INT;
		SELECT @photoID = PhotoID FROM tbl_PlayerVotePhoto WHERE VoteID = @voteID

		--Confirm the voting is not already complete on the photo record
		DECLARE @isVotingCompleted BIT;
		SELECT @isVotingCompleted = IsVotingComplete FROM tbl_Photo WHERE PhotoID = @photoID
		IF(@isVotingCompleted = 1)
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTEALREADYCOMPLETE;
			SET @errorMSG = 'The PlayerVotePhoto record has already been completed.';
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
			
			--Update the PlayerVotePhoto record with the decision
			UPDATE tbl_PlayerVotePhoto
			SET IsPhotoSuccessful = @isPhotoSuccessful
			WHERE VoteID = @voteID

			--Update the photo record with the number of Yes or No Votes
			DECLARE @countYes INT = 0;
			DECLARE @countNo INT = 0;
			SELECT @countYes = COUNT(*) FROM tbl_PlayerVotePhoto WHERE PhotoID = @photoID AND IsPhotoSuccessful = 1
			SELECT @countNo = COUNT(*) FROM tbl_PlayerVotePhoto WHERE PhotoID = @photoID AND IsPhotoSuccessful = 0
			UPDATE tbl_Photo
			SET NumYesVotes = @countYes, NumNoVotes = @countNo
			WHERE PhotoID = @photoID


			--Update the photo's IsVotingComplete field if all the player votes have successfully been completed
			IF NOT EXISTS (SELECT * FROM tbl_PlayerVotePhoto WHERE PhotoID = @photoID AND IsPhotoSuccessful IS NULL AND PlayerVotePhotoIsActive = 1 AND PlayerVotePhotoIsDeleted = 0)
			BEGIN
				UPDATE tbl_Photo
				SET IsVotingComplete = 1
				WHERE PhotoID = @photoID

				-- if successful vote
				IF (@countYes > @countNo)
				BEGIN
					-- updating kills and deaths per players in the photo
					UPDATE tbl_Player SET NumKills = NumKills +1 WHERE PlayerID = (SELECT TakenByPlayerID FROM tbl_Photo WHERE takenByPlayerID = @playerID)
					UPDATE tbl_Player SET NumDeaths = NumDeaths +1 WHERE PlayerID = (SELECT PhotoOfPlayerID FROM tbl_Photo WHERE takenByPlayerID = @playerID)
				END
			END
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_PlayerVoteJoinTables WHERE VoteID = @voteID
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