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


DROP DATABASE Hangfire
GO


CREATE DATABASE Hangfire
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
	GameCode VARCHAR(255) NOT NULL DEFAULT 'ABC123' UNIQUE,
	NumOfPlayers INT NOT NULL DEFAULT 0,
	GameMode VARCHAR(255) DEFAULT 'CORE',
	StartTime DATETIME2 DEFAULT GETDATE(),
	EndTime DATETIME2 DEFAULT DATEADD(DAY, 1, GETDATE()),
	GameState VARCHAR(255) NOT NULL DEFAULT 'STARTING',
	IsJoinableAtAnytime BIT NOT NULL DEFAULT 0,
	GameIsActive BIT NOT NULL DEFAULT 1,

	PRIMARY KEY (GameID),

	CHECK(LEN(GameCode) = 6),
	CHECK(NumOfPlayers >= 0),
	CHECK(GameMode IN ('CORE')),
	CHECK(StartTime < EndTime),
	CHECK(GameState IN ('STARTING', 'PLAYING', 'COMPLETED'))
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
	SelfieDataURL VARCHAR(MAX) NOT NULL,
	NumKills INT NOT NULL DEFAULT 0,
	NumDeaths INT NOT NULL DEFAULT 0,
	NumPhotosTaken INT NOT NULL DEFAULT 0,
	IsHost BIT NOT NULL DEFAULT 0,
	IsVerified BIT NOT NULL DEFAULT 0,
	VerificationCode INT,
	ConnectionID VARCHAR(255) DEFAULT NULL,
	IsConnected BIT NOT NULL DEFAULT 0,
	HasLeftGame BIT NOT NULL DEFAULT 0,
	PlayerIsActive BIT NOT NULL DEFAULT 1,
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
	PhotoDataURL VARCHAR(MAX) NOT NULL,
	TimeTaken DATETIME2 NOT NULL DEFAULT GETDATE(),
	VotingFinishTime DATETIME2 NOT NULL DEFAULT DATEADD(MINUTE, 15, GETDATE()),
	NumYesVotes INT NOT NULL DEFAULT 0,
	NumNoVotes INT NOT NULL DEFAULT 0,
	IsVotingComplete BIT NOT NULL DEFAULT 0,
	PhotoIsActive BIT NOT NULL DEFAULT 1,
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
	IsPhotoSuccessful BIT DEFAULT NULL,
	PlayerVotePhotoIsActive BIT NOT NULL DEFAULT 1,
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
	IsRead BIT NOT NULL DEFAULT 0,
	NotificationIsActive BIT NOT NULL DEFAULT 1,
	GameID INT NOT NULL,
	PlayerID INT NOT NULL,

	PRIMARY KEY (NotificationID),
	FOREIGN KEY (GameID) REFERENCES tbl_Game(GameID),
	FOREIGN KEY (PlayerID) REFERENCES tbl_Player(PlayerID),

	CHECK(NotificationType IN ('VOTE', 'SUCCESS', 'FAIL', 'JOIN', 'LEAVE', 'TAGGED'))
);
GO





-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	A view which joins the Photo. game and player records
-- =============================================
USE udb_CamTag
GO
CREATE VIEW vw_PhotoGameAndPlayers
AS
SELECT 
	PhotoID,
	Lat,
	Long,
	PhotoDataURL,
	TimeTaken,
	VotingFinishTime,
	NumYesVotes,
	NumNoVotes,
	IsVotingComplete,
	PhotoIsActive,
	p.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	GameIsActive,
	TakenByPlayerID,
	takenBy.Nickname AS TakenByPlayerNickname,
	takenBy.Phone AS TakenByPlayerPhone,
	takenBy.Email AS TakenByPlayerEmail,
	takenBy.SelfieDataURL AS TakenByPlayerSelfieDataURL,
	takenBy.NumKills AS TakenByPlayerNumKills,
	takenBy.NumDeaths AS TakenByPlayerNumDeaths,
	takenBy.NumPhotosTaken AS TakenByPlayerNumPhotosTaken,
	takenBy.IsHost AS TakenByPlayerIsHost,
	takenBy.IsVerified AS TakenByPlayerIsVerified,
	takenBy.ConnectionID AS TakenByPlayerConnectionID,
	takenBy.IsConnected AS TakenByPlayerIsConnected,
	takenBy.HasLeftGame AS TakenByPlayerHasLeftGame,
	takenBy.PlayerIsActive AS TakenByPlayerIsActive,
	PhotoOfPlayerID,
	photoOf.Nickname AS PhotoOfPlayerNickname,
	photoOf.Phone AS PhotoOfPlayerPhone,
	photoOf.Email AS PhotoOfPlayerEmail,
	photoOf.SelfieDataURL AS PhotoOfPlayerSelfieDataURL,
	photoOf.NumKills AS PhotoOfPlayerNumKills,
	photoOf.NumDeaths AS PhotoOfPlayerNumDeaths,
	photoOf.NumPhotosTaken AS PhotoOfPlayerNumPhotosTaken,
	photoOf.IsHost AS PhotoOfPlayerIsHost,
	photoOf.IsVerified AS PhotoOfPlayerIsVerified,
	photoOf.ConnectionID AS PhotoOfPlayerConnectionID,
	photoOf.IsConnected AS PhotoOfPlayerIsConnected,
	photoOf.HasLeftGame AS PhotoOfPlayerHasLeftGame,
	photoOf.PlayerIsActive AS PhotoOfPlayerIsActive  
FROM tbl_Photo p
	INNER JOIN tbl_Game g ON (p.GameID = g.GameID)
	INNER JOIN tbl_Player takenBy ON (p.TakenByPlayerID = takenBy.PlayerID)
	INNER JOIN tbl_Player photoOf ON (p.PhotoOfPlayerID = photoOf.PlayerID)
GO







USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Creates a view of Players and their game
-- =============================================
CREATE VIEW vw_PlayerGame
AS
SELECT
	PlayerID,
	Nickname,
	Phone,
	Email,
	SelfieDataURL,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	VerificationCode,
	ConnectionID,
	IsConnected,
	HasLeftGame,
	PlayerIsActive,
	g.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	GameIsActive

FROM tbl_Player p
	INNER JOIN tbl_Game g ON (p.GameID = g.GameID)
GO



-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	A view which contains all the active 
--				games and non complete games as well as 
--				all the active players in the game.
-- =============================================

CREATE VIEW vw_ActiveAndNotCompleteGamesAndPlayers
AS
SELECT
	*
FROM vw_PlayerGame
WHERE
	GameState NOT LIKE 'COMPLETE'
	AND GameIsActive = 1
	AND PlayerIsActive = 1
GO






-- =============================================
-- Author:		Jonathan Williams
-- Create date: 11/09/18
-- Description:	Creates a view of The PlayerVotePhoto records with the joined Player and Photo tables
-- =============================================
USE udb_CamTag
GO
CREATE VIEW vw_PlayerVoteJoinTables
AS
SELECT 
	pvp.VoteID,
	pvp.IsPhotoSuccessful,
	pvp.PlayerVotePhotoIsActive,
	pgp.*,
	pg.PlayerID,
	pg.Nickname,
	pg.Phone,
	pg.Email,
	pg.SelfieDataURL,
	pg.NumKills,
	pg.NumDeaths,
	pg.NumPhotosTaken,
	pg.IsHost,
	pg.IsVerified,
	pg.ConnectionID,
	pg.IsConnected,
	pg.HasLeftGame,
	pg.PlayerIsActive
FROM tbl_PlayerVotePhoto pvp
	INNER JOIN vw_PhotoGameAndPlayers pgp ON (pvp.PhotoID = pgp.PhotoID)
	INNER JOIN vw_PlayerGame pg ON (pvp.PlayerID = pg.PlayerID)
GO




USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 05/09/18
-- Description:	Creates a new game in the database.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. EC_ITEMALREADYEXISTS - An active game already exists with that game code.

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateGame] 
	-- Add the parameters for the stored procedure here
	@gameCode VARCHAR(6),
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_ITEMALREADYEXISTS INT = 14;


	BEGIN TRY  
		--Confirm the game code does not already exist in an active game / currently playing game
		IF EXISTS (SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE GameCode = @gameCode)
		BEGIN
			SET @result = @EC_ITEMALREADYEXISTS;
			SET @errorMSG = 'The game code already exists.';
			RAISERROR('',16,1);
		END;

		--Game code does not exist, create the new game
		DECLARE @createdGameID INT;
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			INSERT INTO tbl_Game (GameCode) VALUES (@gameCode);
			SET @createdGameID = SCOPE_IDENTITY();
		COMMIT

		--Set the success return variables
		SET @result = 1;
		SET @errorMSG = '';


		--Read the new game record
		SELECT * FROM tbl_Game WHERE GameID = @createdGameID
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'The an error occurred while trying to create the game record';
		END

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
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Adds a notification to the DB to then be used.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. The gameID does not exist
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateJoinNotification] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@playerID INT,
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
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the new connectionID does not already exists
		IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @gameID)
		BEGIN
			SET @errorMSG = 'The gameID does not exist';
			RAISERROR('ERROR: gameID does not exist',16,1);
		END;


		--PlayerID exists and connectionID exists, add the notif
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			DECLARE @notifPlayerID INT
			DECLARE @msgTxt VARCHAR(255)
			SELECT @msgTxt = p.Nickname FROM tbl_Player p WHERE p.PlayerID = @PlayerID
			SET @msgTxt += ' has joined the game.'

			DECLARE idCursor CURSOR FOR SELECT PlayerID FROM vw_PlayerGame WHERE GameID = @gameID --open a cursor for the resulting table
			OPEN idCursor

			FETCH NEXT FROM idCursor INTO @notifPlayerID
			WHILE @notifPlayerID != @playerID --@@FETCH_STATUS = 0 --iterate through all players and give them a notif
			BEGIN
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'JOIN', 0, 1, @gameID, @notifPlayerID) -- insert into table with specific playerID
				FETCH NEXT FROM idCursor INTO @notifPlayerID  --iterate to next playerID
			END
			CLOSE idCursor -- close down cursor
			DEALLOCATE idCursor

		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
		END

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
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Adds a notification to the DB to then be used.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. The gameID does not exist
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateLeaveNotification] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@playerID INT,
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
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the new connectionID does not already exists
		IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @gameID)
		BEGIN
			SET @errorMSG = 'The gameID does not exist';
			RAISERROR('ERROR: gameID does not exist',16,1);
		END;


		--PlayerID exists and connectionID exists, add the notif
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			DECLARE @notifPlayerID INT
			DECLARE @msgTxt VARCHAR(255)
			SELECT @msgTxt = p.Nickname FROM tbl_Player p WHERE p.PlayerID = @PlayerID
			SET @msgTxt += ' has left the game.'

			DECLARE idCursor CURSOR FOR SELECT PlayerID FROM vw_PlayerGame WHERE GameID = @gameID --open a cursor for the resulting table
			OPEN idCursor

			FETCH NEXT FROM idCursor INTO @notifPlayerID
			WHILE @notifPlayerID != @playerID --@@FETCH_STATUS = 0 --iterate through all players and give them a notif
			BEGIN
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'LEAVE', 0, 1, @gameID, @notifPlayerID) -- insert into table with specific playerID
				FETCH NEXT FROM idCursor INTO @notifPlayerID  --iterate to next playerID
			END
			CLOSE idCursor -- close down cursor
			DEALLOCATE idCursor

		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
		END

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
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Adds a notification to the DB to then be used.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. The gameID does not exist
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateTaggedNotification] 
	-- Add the parameters for the stored procedure here
	@takenByID INT,
	@photoOfID INT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRY  
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @photoOfID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @takenByID)
		BEGIN
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;


		--PlayerID exists and connectionID exists, add the notif
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION

			DECLARE @msgTxt VARCHAR(255)
			SET @msgTxt = 'You have been tagged by '
			SELECT @msgTxt += p.Nickname FROM tbl_Player p WHERE p.PlayerID = @takenByID
			SET @msgTxt += '.'

			DECLARE @gameID INT
			SELECT @gameID = g.GameID FROM vw_PlayerGame g WHERE g.PlayerID = 100000
			INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'TAGGED', 0, 1, @gameID, @photoOfID) -- insert into table with specific playerID			
		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
		END

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
-- Create date: 05/09/18
-- Description:	Deactivates the game after the host player tried to create a game but failed to join
--				due to an unexpected error such as the email address is already taken by a player in a game etc.
-- =============================================
CREATE PROCEDURE [dbo].[usp_DeactivateGameAfterHostJoinError] 
	-- Add the parameters for the stored procedure here
	@gameID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE tbl_Game
	SET GameIsActive = 0
	WHERE GameID = @gameID

END
GO






-- =============================================
-- Author:		Jonathan Williams
-- Create date: 06/09/18
-- Description:	Gets the game record matching the specified ID
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetGameByID] 
	-- Add the parameters for the stored procedure here
	@gameID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM tbl_Game
	WHERE GameID = @gameID
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
--		1. The playerID passed in does not exist

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
		FROM vw_PlayerGame
		WHERE GameID = @gameID AND PlayerIsActive = 1 AND isVerified = 1

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH

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
-- Author:		Dylan Levin
-- Create date: 06/09/18
-- Description:	Returns all the notifications for a player

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_GetNotifications] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@all BIT,
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

		--The playerID exists, get the notifications associated with that player
		IF (@all = 1)
		SELECT * FROM tbl_Notification WHERE PlayerID = @playerID	
		ELSE
		SELECT * FROM tbl_Notification WHERE PlayerID = @playerID AND IsRead = 0

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH
		
	END CATCH
END
GO






-- =============================================
-- Author:		Jonathan Williams
-- Create date: 10/09/18
-- Description:	Gets the Photo Record matching the specified ID
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPhotoByID] 
	-- Add the parameters for the stored procedure here
	@photoID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM vw_PhotoGameAndPlayers
	WHERE PhotoID = @photoID
	
END
GO






USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_GetPlayerPhotoLocation] 
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
	DECLARE @EC_PHOTOIDDOESNOTEXIST INT = 69;

	BEGIN TRY
		
		

			--Confirm the photoID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Photo WHERE PhotoID = @photoID)
		BEGIN
			SET @result = @EC_PHOTOIDDOESNOTEXIST;
			SET @errorMSG = 'The photoID does not exist';
			RAISERROR('',16,1);
		END

		

		--The playerID exists, get the GameID associated with that player
		DECLARE @gameID INT;
		SELECT @gameID = GameID FROM tbl_Photo WHERE PhotoID = @photoID;

		-- The playerID and photoID exist get the location of the photo
		SELECT *
		FROM tbl_Photo
		WHERE GameID = @gameID AND PhotoIsActive = 1 AND IsVotingComplete = 1 AND PhotoID = @photoID

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''
		
		PRINT 'Hello';

	END TRY

	BEGIN CATCH

	END CATCH

END
GO






-- =============================================
-- Author:		Jonathan Williams
-- Create date: 10/09/18
-- Description:	Gets the PlayerVotePhoto records which the PlayerID must completed / has not voted on yet
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetVotesToComplete] 
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

	--Confirm the playerID passed in exists
	IF NOT EXISTS (SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @playerID)
	BEGIN
		SET @result = @EC_PLAYERIDDOESNOTEXIST;
		SET @errorMSG = 'The playerID does not exist';
		RAISERROR('',16,1);
	END

	SELECT *
	FROM vw_PlayerVoteJoinTables
	WHERE PlayerID = @playerID AND IsPhotoSuccessful IS NULL
	ORDER BY VoteID

	SET @result = 1;
	SET @errorMSG = '';
	
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





-- =============================================
-- Author:		Jonathan Williams
-- Create date: 06/09/18
-- Description:	Removes a players connectionID to the ApplicationHub, outlining
--				that the player is no longer connected to the application hub and
--				receive live updates in the game
-- =============================================
CREATE PROCEDURE [dbo].[usp_RemoveConnectionID] 
	-- Add the parameters for the stored procedure here
	@connectionID VARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE tbl_Player
	SET IsConnected = 0, ConnectionID = NULL
	WHERE ConnectionID LIKE @connectionID
END
GO





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





USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 30/08/18
-- Description:	Updates a Player's ConnectionID and sets them as CONNECTED to the SignalR Hub

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID trying to update does not exist
--		2. The new connectionID already exists inside the database
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_UpdateConnectionID] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@connectionID VARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	BEGIN TRY  
		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		--Confirm the new connectionID does not already exists
		IF EXISTS (SELECT * FROM tbl_Player WHERE ConnectionID = @connectionID)
		BEGIN
			RAISERROR('ERROR: connectionID alread exists',16,1);
		END;


		--PlayerID exists and connectionID does not exists, make the update
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET ConnectionID = @connectionID, IsConnected = 1
			WHERE PlayerID = @playerID
		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
		END

	END CATCH
END
GO





USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_UpdateVerificationCode] 
	-- Add the parameters for the stored procedure here
	@verificationCode INT,
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT,
	@phone VARCHAR(255) OUTPUT,
	@email VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_PLAYERIDDOESNOTEXIST INT = 12;
	

	BEGIN TRY
		
		--Confirm the playerID exists and is in a non verified state
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID AND IsVerified = 0)
		BEGIN
			SET @result = @EC_PLAYERIDDOESNOTEXIST;
			SET @errorMSG = 'The playerID does not exist or is already verified.';
			RAISERROR('',16,1);
		END

		--Update the players verfication code
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET VerificationCode = @verificationCode
			WHERE PlayerID = @playerID
		COMMIT

		--Set the success return variables
		SET @result = 1;
		SET @errorMSG = ''
		SELECT @phone = Phone FROM tbl_Player WHERE PlayerID = @playerID
		SELECT @email = Email FROM tbl_Player WHERE PlayerID = @playerID

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





USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 11/09/18
-- Description:	When the voting time expires on a photo the 
--				photo is automiatically marked as successful.
--				Updating the photo and all the votes because the time has now expired.
-- =============================================
CREATE PROCEDURE [dbo].[usp_VotingTimeExpired] 
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
		
		--Confirm the photo is actual expired
		DECLARE @voteFinish DATETIME2;
		SELECT @voteFinish = VotingFinishTime FROM tbl_Photo WHERE PhotoID = @photoID
		IF(GETDATE() < @voteFinish)
		BEGIN
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to update the photo record.'
			RAISERROR('',16,1);
		END

		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			--Update the photo to completed
			UPDATE tbl_Photo
			SET IsVotingComplete = 1
			WHERE PhotoID = @photoID

			--Since the voting time has expired the photo is an automatic success photo, set all the votes to successful
			UPDATE tbl_PlayerVotePhoto
			SET IsPhotoSuccessful = 1
			WHERE PhotoID = @photoID

			--Update the counts of the votes
			DECLARE @countYesVotes INT;
			SELECT @countYesVotes = COUNT(*) FROM tbl_PlayerVotePhoto WHERE IsPhotoSuccessful = 1 AND PhotoID = @photoID
			UPDATE tbl_Photo
			SET NumYesVotes = @countYesVotes, NumNoVotes = 0
			WHERE PhotoID = @photoID
		COMMIT

		SELECT * FROM vw_PhotoGameAndPlayers WHERE PhotoID = @photoID
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




--Dummy Data
INSERT INTO tbl_Game (GameCode, NumOfPlayers, GameState) VALUES ('tcf124', 6, 'STARTING')
GO

INSERT INTO tbl_Player (Nickname, Phone, SelfieDataURL, GameID, IsVerified) VALUES ('Jono', '+61457558322', 'localhost', 100000, 1)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieDataURL, GameID, IsVerified) VALUES ('Dylan', '+6145620441', 'localhost', 100000, 1)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieDataURL, GameID, IsVerified) VALUES ('Mathew', '+61454758125', 'localhost', 100000, 1)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieDataURL, GameID, IsVerified) VALUES ('Harry', '+61478542569', 'localhost', 100000, 0)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieDataURL, GameID, IsVerified) VALUES ('David', '+61478585269', 'localhost', 100000, 1)
GO
INSERT INTO tbl_Player (Nickname, Phone, SelfieDataURL, GameID, IsVerified) VALUES ('Sheridan', '+61478588547', 'localhost', 100000, 0)
GO
INSERT INTO tbl_Notification (MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES ('ScoMo has joined the game.', 'JOIN', 1, 1, 100000, 100000)
GO
INSERT INTO tbl_Notification (MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES ('test.', 'JOIN', 0, 1, 100000, 100000)

