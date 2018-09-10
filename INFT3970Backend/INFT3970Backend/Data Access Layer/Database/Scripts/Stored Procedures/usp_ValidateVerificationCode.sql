USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_ValidateVerificationCode] 
	-- Add the parameters for the stored procedure here
	@verificationCode INT,
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_PLAYERIDDOESNOTEXIST INT = 12;
	DECLARE @EC_VERIFYPLAYER_CODEINCORRECT INT = 2001;
	

	BEGIN TRY
		
		--Confirm the playerID exists and is in a non verified state
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID AND IsVerified = 0)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The playerID does not exist or is already verified.';
			RAISERROR('',16,1);
		END

		--Confirm the verification code passed in equals the verification code stored against the user account
		DECLARE @actualVerificationCode INT;
		SELECT @actualVerificationCode = VerificationCode FROM tbl_Player WHERE PlayerID = @playerID
		IF(@actualVerificationCode <> @verificationCode)
		BEGIN
			SET @result = @EC_VERIFYPLAYER_CODEINCORRECT;
			SET @errorMSG = 'The verification code is not correct.';
			RAISERROR('',16,1);
		END

		--The verification code is valid, update the player to verified
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET IsVerified = 1
			WHERE PlayerID = @playerID

			--Update the player count in the game which the player belongs to
			DECLARE @gameID INT
			SELECT @gameID = GameID FROM tbl_Player WHERE PlayerID = @playerID

			UPDATE tbl_Game
			SET NumOfPlayers = NumOfPlayers + 1
			WHERE GameID = @gameID
		COMMIT

		--Set the success return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying update the player record';
		END
	END CATCH
END
GO