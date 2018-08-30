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
	ConnectionID VARCHAR(255) DEFAULT NULL,
	IsConnected BIT NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL DEFAULT 0,
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



SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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

	BEGIN TRY  
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @result = 0;
			SET @errorMSG = 'The playerID you are trying to update does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the new connectionID does not already exists
		IF EXISTS (SELECT * FROM tbl_Player WHERE ConnectionID = @connectionID)
		BEGIN
			SET @result = 0;
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
			SET @result = 0;
			SET @errorMSG = 'The an error occurred while trying to save your changes in the database';
		END

	END CATCH
END
GO




--Dummy Data
INSERT INTO tbl_Game (GameCode, NumOfPlayers) VALUES ('tcf124', 3)
GO

INSERT INTO tbl_Player (Nickname, Phone, SelfieFilePath, GameID) VALUES ('Jono', '+61457558322', 'localhost', 100000)
GO



