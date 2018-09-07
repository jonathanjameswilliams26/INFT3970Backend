USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_SavePhoto] 
	-- Add the parameters for the stored procedure here
	@dataURL VARCHAR(MAX), 
	@takenByID INT,
	@photoOfID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_PLAYERIDDOESNOTEXIST INT = 12;
	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY
		--Check the takenByID exists
		IF NOT EXISTS (SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @takenByID)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The taken by PlayerID passed in does not exist or is not active.';
			RAISERROR('',16,1);
		END


		--Check the photoOfID exists
		IF NOT EXISTS (SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @photoOfID)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The photo of PlayerID passed in does not exist or is not active.';
			RAISERROR('',16,1);
		END


		--Get the gameID of the players
		DECLARE @gameID INT;
		SELECT @gameID = GameID FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @takenByID

		--Insert the new photo
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			INSERT INTO tbl_Photo(PhotoDataURL, TakenByPlayerID, PhotoOfPlayerID, GameID) VALUES (@dataURL, @takenByID, @photoOfID, @gameID);

			DECLARE @createdPhotoID INT;
			SET @createdPhotoID = SCOPE_IDENTITY();

			--Create the voting records for all the players in the game. Only active and verified players and players who have not left the game
			INSERT INTO tbl_PlayerVotePhoto(PlayerID, PhotoID)
			SELECT PlayerID, @createdPhotoID
			FROM vw_ActiveAndNotCompleteGamesAndPlayers
			WHERE GameID = @gameID AND IsVerified = 1 AND HasLeftGame = 0
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to save the photo.'
		END
	END CATCH
END
GO