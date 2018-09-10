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


		--Confirm the game is not completed
		DECLARE @gameState VARCHAR(255);
		SELECT @gameState = GameState FROM tbl_Game WHERE GameID = @gameID
		IF(@gameState LIKE 'COMPLETED')
		BEGIN
			SET @result = @EC_GAMEALREADYCOMPLETE;
			SET @errorMSG = 'The game is already completed.';
			RAISERROR('',16,1);
		END

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


		SELECT * FROM vw_PhotoGameAndPlayers WHERE PhotoID = @createdPhotoID
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