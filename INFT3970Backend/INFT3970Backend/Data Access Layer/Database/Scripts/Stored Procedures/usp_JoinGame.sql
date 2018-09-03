USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_JoinGame] 
	-- Add the parameters for the stored procedure here
	@gameCode VARCHAR(6),
	@nickname VARCHAR(255),
	@contact VARCHAR(255),
	@isPhone BIT,
	@verificationCode INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT,
	@createdPlayerID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_GAMEDOESNOTEXIST INT = 13;
	DECLARE @EC_JOINGAME_GAMEALREADYCOMPLETE INT = 1004;
	DECLARE @EC_JOINGAME_NICKNAMETAKEN INT = 1005;
	DECLARE @EC_JOINGAME_PHONETAKEN INT = 1006;
	DECLARE @EC_JOINGAME_EMAILTAKEN INT = 1007;

	BEGIN TRY
		
		--Confirm the gameCode passed in exists
		DECLARE @gameIDToJoin INT;
		SELECT @gameIDToJoin = GameID FROM tbl_Game WHERE GameCode = @gameCode
		IF(@gameIDToJoin IS NULL)
		BEGIN
			SET @result = @EC_GAMEDOESNOTEXIST;
			SET @errorMSG = 'The game code does not exist';
			RAISERROR('',16,1);
		END

		--Confirm the game you are trying to join is not completed
		DECLARE @isGameAlreadyCompleted BIT;
		SELECT @isGameAlreadyCompleted = isComplete FROM tbl_Game WHERE GameID = @gameIDToJoin
		IF(@isGameAlreadyCompleted = 1)
		BEGIN
			SET @result = @EC_JOINGAME_GAMEALREADYCOMPLETE;
			SET @errorMSG = 'The game you are trying to join is already completed.';
			RAISERROR('',16,1);
		END


		--TODO: add the business logic to determine if the game can be joined at anytime?
		--Check IsJoinableAnyTime
		--Check the gameState == IN LOBBY


		--Confirm the nickname entered is not already taken by a player in the game
		IF EXISTS (SELECT * FROM tbl_Player WHERE GameID = @gameIDToJoin AND Nickname = @nickname AND IsActive = 1)
		BEGIN
			SET @result = @EC_JOINGAME_NICKNAMETAKEN;
			SET @errorMSG = 'The nickname you entered is already taken. Please chose another';
			RAISERROR('',16,1);
		END


		--Confirm if the phone number or email address is already taken by a player in the game
		IF(@isPhone = 1)
		BEGIN
			--Confirm the phone number is unique in the game
			IF EXISTS(SELECT Phone FROM tbl_Player WHERE GameID = @gameIDToJoin AND Phone LIKE @contact AND IsActive = 1)
			BEGIN
				SET @result = @EC_JOINGAME_PHONETAKEN;
				SET @errorMSG = 'The phone number you entered is already taken by another player in the game. Please enter a unique contact.';
				RAISERROR('',16,1);
			END
		END
		ELSE
		BEGIN
			--Confirm the email is unique in the game
			IF EXISTS(SELECT Email FROM tbl_Player WHERE GameID = @gameIDToJoin AND Email LIKE @contact AND IsActive = 1)
			BEGIN
				SET @result = @EC_JOINGAME_EMAILTAKEN;
				SET @errorMSG = 'The email address you entered is already taken by another player in the game. Please enter a unique contact.';
				RAISERROR('',16,1);
			END
		END

		--If reaching this point all the inputs have been validated. Create the player record
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			IF(@isPhone = 1)
			BEGIN
				INSERT INTO tbl_Player(Nickname, Phone, SelfieFilePath, GameID, VerificationCode) VALUES (@nickname, @contact, 'no selfie', @gameIDToJoin, @verificationCode);
			END
			ELSE
			BEGIN
				INSERT INTO tbl_Player(Nickname, Email, SelfieFilePath, GameID, VerificationCode) VALUES (@nickname, @contact, 'no selfie', @gameIDToJoin, @verificationCode);
			END

			SET @createdPlayerID = SCOPE_IDENTITY();
		COMMIT

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to add the player to the game'
		END
		SET @createdPlayerID = -1;
	END CATCH
END
GO