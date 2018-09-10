USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Joins a player to a game.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. @EC_GAMEDOESNOTEXIST - The game code passed in does not exist in the database or is not an active game.
--		3. @EC_GAMEALREADYCOMPLETE - The game code passed in trying to join is already completed
--		4. @EC_JOINGAME_NICKNAMETAKEN - The nickname passed is already taken in the game
--		5. @EC_JOINGAME_PHONETAKEN - The phone number is already taken in another active game
--		6. @EC_JOINGAME_EMAILTAKEN - The email is already taken in another active game
--		7. @EC_JOINGAME_UNABLETOJOIN - The game the player is trying to join is already playing and does not allow players to join mid game

-- =============================================
CREATE PROCEDURE [dbo].[usp_JoinGame] 
	-- Add the parameters for the stored procedure here
	@gameCode VARCHAR(6),
	@nickname VARCHAR(255),
	@contact VARCHAR(255),
	@isPhone BIT,
	@verificationCode INT,
	@isHost BIT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_GAMEDOESNOTEXIST INT = 13;
	DECLARE @EC_GAMEALREADYCOMPLETE INT = 16;
	DECLARE @EC_JOINGAME_NICKNAMETAKEN INT = 1005;
	DECLARE @EC_JOINGAME_PHONETAKEN INT = 1006;
	DECLARE @EC_JOINGAME_EMAILTAKEN INT = 1007;
	DECLARE @EC_JOINGAME_UNABLETOJOIN INT = 1008;

	BEGIN TRY
		
		DECLARE @createdPlayerID INT;

		--Confirm the gameCode passed in exists and is active
		DECLARE @gameIDToJoin INT;
		SELECT @gameIDToJoin = GameID FROM tbl_Game WHERE GameCode = @gameCode AND GameIsActive = 1
		IF(@gameIDToJoin IS NULL)
		BEGIN
			SET @result = @EC_GAMEDOESNOTEXIST;
			SET @errorMSG = 'The game code does not exist';
			RAISERROR('',16,1);
		END

		--Confirm the game you are trying to join is not completed
		DECLARE @gameState VARCHAR(255);
		SELECT @gameState = GameState FROM tbl_Game WHERE GameID = @gameIDToJoin
		IF(@gameState LIKE 'COMPLETED')
		BEGIN
			SET @result = @EC_GAMEALREADYCOMPLETE;
			SET @errorMSG = 'The game you are trying to join is already completed.';
			RAISERROR('',16,1);
		END

		--Check to see if the player is okay to join the game, as the player may be attempting to join a game which has already begun
		DECLARE @canJoin BIT;
		SELECT @canJoin = IsJoinableAtAnytime FROM tbl_Game WHERE GameID = @gameIDToJoin

		--If the player cannot join at anytime and the current game state is currently PLAYING, return an error because the player
		--is trying to join a game which has already begun and does not allow players to join at anytime
		IF(@canJoin = 0 AND @gameState LIKE 'PLAYING')
		BEGIN
			SET @result = @EC_JOINGAME_UNABLETOJOIN;
			SET @errorMSG = 'The game you are trying to join is already playing and does not allow you to join after the game has started.';
			RAISERROR('',16,1);
		END



		--Confirm the nickname entered is not already taken by a player in the game
		IF EXISTS (SELECT * FROM tbl_Player WHERE GameID = @gameIDToJoin AND Nickname = @nickname AND PlayerIsActive = 1)
		BEGIN
			SET @result = @EC_JOINGAME_NICKNAMETAKEN;
			SET @errorMSG = 'The nickname you entered is already taken. Please chose another';
			RAISERROR('',16,1);
		END


		--Confirm if the phone number or email address is not already taken by a player in another active game
		IF(@isPhone = 1)
		BEGIN
			--Confirm the phone number is unique in all active / not complete games
			IF EXISTS(SELECT Phone FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE Phone LIKE @contact)
			BEGIN
				SET @result = @EC_JOINGAME_PHONETAKEN;
				SET @errorMSG = 'The phone number you entered is already taken by another player in an active/not complete game. Please enter a unique contact.';
				RAISERROR('',16,1);
			END
		END
		ELSE
		BEGIN
			--Confirm the email is unique in the game
			IF EXISTS(SELECT Email FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE Email LIKE @contact)
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
				INSERT INTO tbl_Player(Nickname, Phone, SelfieDataURL, GameID, VerificationCode, IsHost) VALUES (@nickname, @contact, 'no selfie', @gameIDToJoin, @verificationCode, @isHost);
			END
			ELSE
			BEGIN
				INSERT INTO tbl_Player(Nickname, Email, SelfieDataURL, GameID, VerificationCode, IsHost) VALUES (@nickname, @contact, 'no selfie', @gameIDToJoin, @verificationCode, @isHost);
			END

			SET @createdPlayerID = SCOPE_IDENTITY();
		COMMIT

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

		--Read the player record
		SELECT * FROM vw_PlayerGame WHERE PlayerID = @createdPlayerID

	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to add the player to the game'
		END
	END CATCH
END
GO