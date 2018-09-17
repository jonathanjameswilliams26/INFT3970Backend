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
	@lat FLOAT,
	@long FLOAT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_PLAYERIDDOESNOTEXIST INT = 12;
	DECLARE @EC_GAMEALREADYCOMPLETE INT = 16;
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_DATAINVALID INT = 17;

	BEGIN TRY
		--Check the takenByID exists
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @takenByID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Check the photoOfID exists
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @photoOfID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Get the gameID of the players
		DECLARE @gameID INT;
		SELECT @gameID = GameID FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @takenByID


		--Get the GameID of the TakenByPLayerID and PhotoOfPlayerID and confirm they are in the same game
		DECLARE @takenByGameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @takenByID, @gameID = @takenByGameID OUTPUT
		DECLARE @photoOfGameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @takenByID, @gameID = @photoOfGameID OUTPUT
		IF(@takenByGameID <> @photoOfGameID)
		BEGIN
			SET @result = @EC_DATAINVALID;
			SET @errorMSG = 'The players provided are not in the same game.'
		END

		--Confirm the game is not completed
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @takenByGameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Insert the new photo
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			INSERT INTO tbl_Photo(PhotoDataURL, TakenByPlayerID, PhotoOfPlayerID, GameID, Lat, Long) VALUES (@dataURL, @takenByID, @photoOfID, @gameID, @lat, @long);

			DECLARE @createdPhotoID INT;
			SET @createdPhotoID = SCOPE_IDENTITY();

			--Create the voting records for all the players in the game. Only active/verified players, players who have not left the game and not the player who took the photo and who the photo is off.
			INSERT INTO tbl_PlayerVotePhoto(PlayerID, PhotoID)
			SELECT PlayerID, @createdPhotoID
			FROM vw_ActiveAndNotCompleteGamesAndPlayers
			WHERE GameID = @gameID AND IsVerified = 1 AND HasLeftGame = 0 AND PlayerID <> @photoOfID AND PlayerID <> @takenByID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_PhotoGameAndPlayers WHERE PhotoID = @createdPhotoID
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