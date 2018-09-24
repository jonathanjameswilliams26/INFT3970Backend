USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 11/09/18
-- Description:	Updates the Yes/No vote count on the photo,
--				checks if the voting has been completed, if so,
--				the kills and deaths are updated etc.

-- Returns: 1 = Successful, or Anything else = An error occurred
-- =============================================
CREATE PROCEDURE [dbo].[usp_UpdateVotingCountOnPhoto] 
	-- Add the parameters for the stored procedure here 
	@photoID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
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
	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to update the photo record.'
		END
	END CATCH
END
GO