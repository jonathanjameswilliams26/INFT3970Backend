/*
-----------------------------------------------------------------
Script Name: CreateDB

Description:    Creates the database tables, views and stored procedures
				required to run the CamTag web application

Course: INFT3970 - IT Major Project - CamTag

Author:         Jonathan Williams - 3237808
-----------------------------------------------------------------
*/

--Drop the database if it exists
DROP DATABASE udb_CamTag
GO

--Create the Database
CREATE DATABASE udb_CamTag
GO

--Creating a login to use with the connection string
CREATE LOGIN DataAccessLayerLogin WITH PASSWORD = 'test'
GO

USE udb_CamTag
GO


--Creating a user for the login
CREATE USER DataAccessLayerUser FOR LOGIN DataAccessLayerLogin;
GO

--Creating a role which only has select, insert and update permission since the login used for the web app should only have those permissions
--Adding the newly created user to the new role
CREATE ROLE DataAccessLayerRole;
EXEC sp_addrolemember @rolename = DataAccessLayerRole, @membername = DataAccessLayerUser;
GO

--Granting and deny specific permissions to user
GRANT SELECT, UPDATE, INSERT to DataAccessLayerRole;
GO

GRANT EXECUTE TO DataAccessLayerRole;
GO

DENY DELETE, ALTER to DataAccessLayerRole;
GO


--Create the Game table
CREATE TABLE tbl_Game
(
	GameID INT NOT NULL IDENTITY(100000, 1),
	GameCode VARCHAR(255) NOT NULL DEFAULT 'ABC123',
	NumOfPlayers INT NOT NULL DEFAULT 0,
	GameMode VARCHAR(255) DEFAULT 'CORE',
	StartTime DATETIME2 DEFAULT GETDATE(),
	EndTime DATETIME2 DEFAULT DATEADD(DAY, 1, GETDATE()),
	IsComplete BIT NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL DEFAULT 1,

	PRIMARY KEY (GameID),

	CHECK(LEN(GameCode) = 6),
	CHECK(NumOfPlayers >= 0),
	CHECK(GameMode IN ('CORE')),
	CHECK (StartTime < EndTime)
);
GO



--Create a function which will be used as a check constraint to verify
-- a players contact, either phone or email must be provided, both cannot be null
CREATE FUNCTION udf_IsPlayerEmailAndPhoneValid (
  @phone VARCHAR(12),
  @email VARCHAR(255)
)
RETURNS tinyint
AS
BEGIN
  DECLARE @Result tinyint;
  IF (@phone IS NULL AND @email IS NULL)
    SET @Result= 0
  ELSE 
    SET @Result= 1
  RETURN @Result
END
GO



--Create the Player table
CREATE TABLE tbl_Player
(
	PlayerID INT NOT NULL IDENTITY(100000, 1),
	Nickname VARCHAR(255) NOT NULL DEFAULT 'Player',
	Phone VARCHAR(12),
	Email VARCHAR(255),
	SelfieFilePath VARCHAR(255) NOT NULL,
	NumKills INT NOT NULL DEFAULT 0,
	NumDeaths INT NOT NULL DEFAULT 0,
	NumPhotosTaken INT NOT NULL DEFAULT 0,
	IsHost BIT NOT NULL DEFAULT 0,
	IsVerified BIT NOT NULL DEFAULT 0,
	VerificationCode INT,
	ConnectionID VARCHAR(255) DEFAULT NULL,
	IsConnected BIT NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL DEFAULT 1,
	GameID INT NOT NULL,

	PRIMARY KEY (PlayerID),
	FOREIGN KEY (GameID) REFERENCES tbl_Game(GameID),

	CHECK(NumKills >= 0),
	CHECK(NumDeaths >= 0),
	CHECK(NumPhotosTaken >= 0),
	CONSTRAINT IsContactProvided CHECK 
	(
		dbo.udf_IsPlayerEmailAndPhoneValid(Phone, Email) = 1
	)
);
GO





--Create the Photo table
CREATE TABLE tbl_Photo 
(
	PhotoID INT NOT NULL IDENTITY(100000, 1),
	Lat FLOAT,
	Long FLOAT,
	FilePath VARCHAR(255) NOT NULL,
	TimeTaken DATETIME2 NOT NULL DEFAULT GETDATE(),
	VotingFinishTime DATETIME2 NOT NULL DEFAULT DATEADD(MINUTE, 15, GETDATE()),
	NumYesVotes INT NOT NULL DEFAULT 0,
	NumNoVotes INT NOT NULL DEFAULT 0,
	IsVotingComplete BIT NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL DEFAULT 1,
	GameID INT NOT NULL,
	TakenByPlayerID INT NOT NULL,
	PhotoOfPlayerID INT NOT NULL,

	PRIMARY KEY (PhotoID),
	FOREIGN KEY (GameID) REFERENCES tbl_Game(GameID),
	FOREIGN KEY (TakenByPlayerID) REFERENCES tbl_Player(PlayerID),
	FOREIGN KEY (PhotoOfPlayerID) REFERENCES tbl_Player(PlayerID),

	CHECK(VotingFinishTime > TimeTaken),
	CHECK(NumYesVotes >= 0),
	CHECK(NumNoVotes >= 0)
);
GO




--Create the Player and Photo many to many relationship
CREATE TABLE tbl_PlayerVotePhoto
(
	VoteID INT NOT NULL IDENTITY(100000, 1),
	IsPhotoSuccessful BIT NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL DEFAULT 1,
	PhotoID INT NOT NULL,
	PlayerID INT NOT NULL,

	PRIMARY KEY (VoteID),
	FOREIGN KEY (PhotoID) REFERENCES tbl_Photo(PhotoID),
	FOREIGN KEY (PlayerID) REFERENCES tbl_Player(PlayerID)
);
GO




--Create the Notification table
CREATE TABLE tbl_Notification
(
	NotificationID INT NOT NULL IDENTITY(100000, 1),
	MessageText VARCHAR(255) NOT NULL DEFAULT 'Notification',
	NotificationType VARCHAR(255) NOT NULL DEFAULT 'VOTE',
	Link VARCHAR(255),
	IsRead BIT NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL DEFAULT 1,
	GameID INT NOT NULL,
	PlayerID INT NOT NULL,

	PRIMARY KEY (NotificationID),
	FOREIGN KEY (GameID) REFERENCES tbl_Game(GameID),
	FOREIGN KEY (PlayerID) REFERENCES tbl_Player(PlayerID),

	CHECK(NotificationType IN ('VOTE', 'SUCCESS', 'FAIL'))
);
GO


CREATE PROCEDURE [dbo].[usp_UpdateConnectionID] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@connectionID VARCHAR(255),
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
	DECLARE @EC_ITEMALREADYEXISTS INT = 14;


	BEGIN TRY  
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The playerID you are trying to update does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the new connectionID does not already exists
		IF EXISTS (SELECT * FROM tbl_Player WHERE ConnectionID = @connectionID)
		BEGIN
			SET @result = @EC_ITEMALREADYEXISTS;
			SET @errorMSG = 'The connectionID you are trying to update already exists';
			RAISERROR('ERROR: connectionID alread exists',16,1);
		END;


		--PlayerID exists and connectionID does not exists, make the update
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET ConnectionID = @connectionID, IsConnected = 1
			WHERE PlayerID = @playerID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'The an error occurred while trying to save your changes in the database';
		END

	END CATCH
END
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




USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 1/09/18
-- Description:	Gets all the players in a game, takes in a playerID and users that playerID to find all other players in the game

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID trying to update does not exist

-- =============================================
CREATE PROCEDURE [dbo].[usp_GetGamePlayerList] 
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

	BEGIN TRY
		
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('',16,1);
		END

		--The playerID exists, get the GameID associated with that player
		DECLARE @gameID INT;
		SELECT @gameID = GameID FROM tbl_Player WHERE PlayerID = @playerID;

		--Get all the active and verified players inside that game
		SELECT *
		FROM tbl_Player
		WHERE GameID = @gameID AND IsActive = 1 AND isVerified = 1

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH

	END CATCH

END
GO




--Dummy Data
INSERT INTO tbl_Game (GameCode, NumOfPlayers) VALUES ('tcf124', 3)
GO

INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('Jono', '+61457558322', 'localhost', 100000)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('Dylan', '+61485471258', 'localhost', 100000)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('Mathew', '+61454758125', 'localhost', 100000)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('Harry', '+61478542569', 'localhost', 100000)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('David', '+61478585269', 'localhost', 100000)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('Sheridan', '+61478588547', 'localhost', 100000)
GO



