/*
-----------------------------------------------------------------
Script Name: CreateDB

Description:    Creates the database tables, views and stored procedures
				required to run the CamTag web application

Course: INFT3970 - IT Major Project - CamTag

Author:         Jonathan Williams - 3237808
-----------------------------------------------------------------
*/

--Creating a login to use with the connection string
CREATE LOGIN DataAccessLayerLogin WITH PASSWORD = 'test'
GO
EXEC master..sp_addsrvrolemember @loginame = N'DataAccessLayerLogin', @rolename = N'sysadmin'
GO


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



USE udb_CamTag
GO


--Creating a user for the login
CREATE USER DataAccessLayerUser FOR LOGIN DataAccessLayerLogin;
GO

--Creating a role which only has select, insert and update permission since the login used for the web app should only have those permissions
--Adding the newly created user to the new role
--CREATE ROLE DataAccessLayerRole;
--GO
--EXEC sp_addrolemember @rolename = DataAccessLayerRole, @membername = DataAccessLayerUser;
--GO
--GRANT SELECT, UPDATE, INSERT, DELETE to DataAccessLayerRole;
--GO

--GRANT EXECUTE TO DataAccessLayerRole;
--GO


USE Hangfire
GO
CREATE USER DataAccessLayerUser FOR LOGIN DataAccessLayerLogin;
GO
--CREATE ROLE DataAccessLayerRole;
--GO
--EXEC sp_addrolemember @rolename = DataAccessLayerRole, @membername = DataAccessLayerUser;
--GO

----Granting and deny specific permissions to user
--GRANT SELECT, UPDATE, INSERT, DELETE to DataAccessLayerRole;
--GO

--GRANT EXECUTE TO DataAccessLayerRole;
--GO


USE udb_CamTag
GO

--Create the Game table
CREATE TABLE tbl_Game
(
	GameID INT NOT NULL IDENTITY(100000, 1),
	GameCode VARCHAR(255) NOT NULL DEFAULT 'ABC123' UNIQUE,
	NumOfPlayers INT NOT NULL DEFAULT 0,
	GameMode VARCHAR(255) DEFAULT 'CORE',
	StartTime DATETIME2,
	EndTime DATETIME2,
	GameState VARCHAR(255) NOT NULL DEFAULT 'IN LOBBY',
	IsJoinableAtAnytime BIT NOT NULL DEFAULT 0,
	GameIsActive BIT NOT NULL DEFAULT 1,
	GameIsDeleted BIT NOT NULL DEFAULT 0,

	PRIMARY KEY (GameID),

	CHECK(LEN(GameCode) = 6),
	CHECK(NumOfPlayers >= 0),
	CHECK(GameMode IN ('CORE')),
	CHECK(StartTime < EndTime),
	CHECK(GameState IN ('IN LOBBY', 'STARTING', 'PLAYING', 'COMPLETED'))
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
	AmmoCount INT NOT NULL DEFAULT 3,
	NumKills INT NOT NULL DEFAULT 0,
	NumDeaths INT NOT NULL DEFAULT 0,
	NumPhotosTaken INT NOT NULL DEFAULT 0,
	IsHost BIT NOT NULL DEFAULT 0,
	IsVerified BIT NOT NULL DEFAULT 0,
	VerificationCode INT,
	ConnectionID VARCHAR(255) DEFAULT NULL,
	IsConnected BIT NOT NULL DEFAULT 0,
	HasLeftGame BIT NOT NULL DEFAULT 0,
	PlayerIsDeleted BIT NOT NULL DEFAULT 0,
	PlayerIsActive BIT NOT NULL DEFAULT 1,
	GameID INT NOT NULL,

	PRIMARY KEY (PlayerID),
	FOREIGN KEY (GameID) REFERENCES tbl_Game(GameID),

	CHECK(NumKills >= 0),
	CHECK(NumDeaths >= 0),
	CHECK(NumPhotosTaken >= 0),
	CHECK(AmmoCount >= 0),
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
	PhotoIsDeleted BIT NOT NULL DEFAULT 0,
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
	PlayerVotePhotoIsDeleted BIT NOT NULL DEFAULT 0,
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
	NotificationType VARCHAR(255) NOT NULL DEFAULT 'SUCCESS',
	IsRead BIT NOT NULL DEFAULT 0,
	NotificationIsActive BIT NOT NULL DEFAULT 1,
	NotificationIsDeleted BIT NOT NULL DEFAULT 0,
	GameID INT NOT NULL,
	PlayerID INT NOT NULL,

	PRIMARY KEY (NotificationID),
	FOREIGN KEY (GameID) REFERENCES tbl_Game(GameID),
	FOREIGN KEY (PlayerID) REFERENCES tbl_Player(PlayerID),

	CHECK(NotificationType IN ('SUCCESS', 'FAIL', 'JOIN', 'LEAVE', 'AMMO'))
);
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
	AmmoCount,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	PlayerIsActive,
	PlayerIsDeleted,
	HasLeftGame,
	ConnectionID,
	IsConnected,
	g.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	GameIsActive,
	GameIsDeleted
FROM tbl_Player p
	INNER JOIN tbl_Game g ON (p.GameID = g.GameID)
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
	PhotoIsDeleted,
	p.GameID,
	GameCode,
	NumOfPlayers,
	GameMode,
	StartTime,
	EndTime,
	GameState,
	IsJoinableAtAnytime,
	GameIsActive,
	GameIsDeleted,
	TakenByPlayerID,
	takenBy.Nickname AS TakenByPlayerNickname,
	takenBy.Phone AS TakenByPlayerPhone,
	takenBy.Email AS TakenByPlayerEmail,
	takenBy.SelfieDataURL AS TakenByPlayerSelfieDataURL,
	takenBy.AmmoCount AS TakenByPlayerAmmoCount,
	takenBy.NumKills AS TakenByPlayerNumKills,
	takenBy.NumDeaths AS TakenByPlayerNumDeaths,
	takenBy.NumPhotosTaken AS TakenByPlayerNumPhotosTaken,
	takenBy.IsHost AS TakenByPlayerIsHost,
	takenBy.IsVerified AS TakenByPlayerIsVerified,
	takenBy.ConnectionID AS TakenByPlayerConnectionID,
	takenBy.IsConnected AS TakenByPlayerIsConnected,
	takenBy.PlayerIsActive AS TakenByPlayerIsActive,
	takenBy.PlayerIsDeleted AS TakenByPlayerIsDeleted,
	takenBy.HasLeftGame AS TakenByPlayerHasLeftGame,
	PhotoOfPlayerID,
	photoOf.Nickname AS PhotoOfPlayerNickname,
	photoOf.Phone AS PhotoOfPlayerPhone,
	photoOf.Email AS PhotoOfPlayerEmail,
	photoOf.SelfieDataURL AS PhotoOfPlayerSelfieDataURL,
	photoOf.AmmoCount AS PhotoOfPlayerAmmoCount,
	photoOf.NumKills AS PhotoOfPlayerNumKills,
	photoOf.NumDeaths AS PhotoOfPlayerNumDeaths,
	photoOf.NumPhotosTaken AS PhotoOfPlayerNumPhotosTaken,
	photoOf.IsHost AS PhotoOfPlayerIsHost,
	photoOf.IsVerified AS PhotoOfPlayerIsVerified,
	photoOf.ConnectionID AS PhotoOfPlayerConnectionID,
	photoOf.IsConnected AS PhotoOfPlayerIsConnected,
	photoOf.PlayerIsActive AS PhotoOfPlayerIsActive,
	photoOf.PlayerIsDeleted AS PhotoOfPlayerIsDeleted,
	photoOf.HasLeftGame AS PhotoOfPlayerHasLeftGame  
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
-- Description:	Creates a view of all Active, Non Deleted and Verfied Players
-- =============================================
CREATE VIEW vw_ActiveAndVerifiedPlayers
AS
SELECT
	PlayerID,
	Nickname,
	Phone,
	Email,
	SelfieDataURL,
	AmmoCount,
	NumKills,
	NumDeaths,
	NumPhotosTaken,
	IsHost,
	IsVerified,
	VerificationCode,
	ConnectionID,
	IsConnected,
	HasLeftGame,
	PlayerIsDeleted,
	PlayerIsActive,
	GameID

FROM tbl_Player
WHERE
	PlayerIsActive = 1 AND
	PlayerIsDeleted = 0 AND
	IsVerified = 1
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
	AND PlayerIsDeleted = 0
	AND GameIsDeleted = 0
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
	pvp.PlayerVotePhotoIsDeleted,
	pgp.*,
	pg.PlayerID,
	pg.Nickname,
	pg.Phone,
	pg.Email,
	pg.SelfieDataURL,
	pg.AmmoCount,
	pg.NumKills,
	pg.NumDeaths,
	pg.NumPhotosTaken,
	pg.IsHost,
	pg.IsVerified,
	pg.ConnectionID,
	pg.IsConnected,
	pg.HasLeftGame,
	pg.PlayerIsActive,
	pg.PlayerIsDeleted
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
CREATE PROCEDURE [dbo].[usp_DoRaiseError] 
	@result INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF(@result <> 1)
	BEGIN
		RAISERROR('',16,1);
	END	
END
GO





USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Get the GameID from the passed in PlayerID
-- =============================================
CREATE PROCEDURE usp_GetGameIDFromPlayer
	@id INT,
	@gameID INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT @gameID = GameID
	FROM tbl_Player
	WHERE PlayerID = @id
END
GO



USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the GameID is active. If the GameID active the procedure will return
--				Otherwise the procedure will raise an error.
-- =============================================
CREATE PROCEDURE usp_ConfirmGameExists
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_GAMEDOESNOTEXIST INT = 13;

    IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @id AND GameIsDeleted = 0)
	BEGIN
		SELECT @result = @EC_GAMEDOESNOTEXIST;
		SET @errorMSG = 'The Game does not exist.';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO




USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the GameID is active. If the GameID active the procedure will return
--				Otherwise the procedure will raise an error.
-- =============================================
CREATE PROCEDURE usp_ConfirmGameIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_GAMENOTACTIVE INT = 9;

    IF NOT EXISTS (SELECT * FROM tbl_Game WHERE GameID = @id AND GameIsActive = 1 AND GameIsDeleted = 0)
	BEGIN
		SELECT @result = @EC_GAMENOTACTIVE;
		SET @errorMSG = 'The Game is not active.';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO






USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the Game exists and is active.
--				Will run normally if the GameID is active and exists
--				Will raise an error and return the error message and result if fails.
-- =============================================
CREATE PROCEDURE usp_ConfirmGameExistsAndIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Call the procedure to confirm the playerID exists
    EXEC [dbo].[usp_ConfirmGameExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT

	IF(@result = 1)
	BEGIN
		--Call the proceduer to confirm the PlayerID is active
		EXEC [dbo].[usp_ConfirmGameIsActive] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	END
END
GO






USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the Game is not already completed.
--				If the game is not complete it will run normally,
--				otherwise, if the game is completed an error will be raised
-- =============================================
CREATE PROCEDURE usp_ConfirmGameNotCompleted
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_GAMEALREADYCOMPLETE INT = 16;

	IF EXISTS (SELECT * FROM tbl_Game WHERE GameID = @id AND GameState = 'COMPLETED')
	BEGIN
		SELECT @result = @EC_GAMEALREADYCOMPLETE;
		SET @errorMSG = 'The game is already completed.';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO







USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the PlayerID exists. If the playerID exists the procedure will return
--				Otherwise the procedure will raise an error.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerExists
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_PLAYERDOESNOTEXIST INT = 12;

    IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @id AND PlayerIsDeleted = 0)
	BEGIN
		SELECT @result = @EC_PLAYERDOESNOTEXIST;
		SET @errorMSG = 'The PlayerID does not exist';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO




USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the PlayerID is active. If the playerID active the procedure will return
--				Otherwise the procedure will raise an error.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_PLAYERNOTACTIVE INT = 10;

    IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @id AND PlayerIsActive = 1 AND PlayerIsDeleted = 0)
	BEGIN
		SELECT @result = @EC_PLAYERNOTACTIVE;
		SET @errorMSG = 'The PlayerID is not active.';
	END
	ELSE
	BEGIN
		SELECT @result = 1;
		SET @errorMSG = '';
	END
END
GO





USE udb_CamTag
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 15/09/18
-- Description:	Confirms the PlayerID exists and is active.
--				Will run normally if the playerID is active and exists
--				Will raise an error and return the error message and result if fails.
-- =============================================
CREATE PROCEDURE usp_ConfirmPlayerExistsAndIsActive
	@id INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Call the procedure to confirm the playerID exists
    EXEC [dbo].[usp_ConfirmPlayerExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT

	IF(@result = 1)
	BEGIN
		--Call the proceduer to confirm the PlayerID is active
		EXEC [dbo].[usp_ConfirmPlayerIsActive] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	END
END
GO




-- =============================================
-- Author:		Jonathan Williams
-- Create date: 06/09/18
-- Description:	Gets the game record matching the specified ID
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetGameByID] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Confirm the GameID passed in exists and is active
	EXEC [dbo].[usp_ConfirmGameExists] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	SELECT *
	FROM tbl_Game
	WHERE GameID = @gameID

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
-- Author:		Dylan Levin
-- Create date: 11/09/18
-- Description:	Adds a notification informing the user that their ammo has refilled

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. EC_PLAYERNOTACTIVE - The playerID passed in is not active
--		2. EC_PLAYERDOESNOTEXIST - The playerID passed in does not exist
--		3. EC_INSERTERROR - When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateAmmoNotification] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT


		--Add the notification after successfully performing the precondition checks
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION		
			INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES ('You have more ammo!', 'AMMO', 0, 1, @gameID, @playerID) -- insert into table with specific playerID			
		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SELECT @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to create the ammo notification.'
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

	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_ITEMALREADYEXISTS INT = 14;

	BEGIN TRY  
		--Confirm the game code does not already exist in a game
		IF EXISTS (SELECT * FROM tbl_Game WHERE GameCode = @gameCode)
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
-- Description:	Adds a Join notification to each player in the game.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. EC_PLAYERNOTACTIVE - The playerID passed in is not active
--		2. EC_PLAYERDOESNOTEXIST - The playerID passed in does not exist
--		3. EC_INSERTERROR - When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateJoinNotification] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the game is not completed
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Add the join notification to each other player in the game.
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION

			--Get the message which will be added as the notification text
			DECLARE @msgTxt VARCHAR(255)
			SELECT @msgTxt = p.Nickname + ' has joined the game.' FROM tbl_Player p WHERE p.PlayerID = @PlayerID


			--Create the notifications for all other players in the game
			INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
			SELECT @msgTxt, 'JOIN', 0, 1, @gameID, PlayerID
			FROM vw_ActiveAndVerifiedPlayers
			WHERE PlayerID <> @playerID AND GameID = @gameID

		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to create the join notification.'
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
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		
		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT

		--Confirm the game is not completed
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--PlayerID exists and connectionID exists, add the notif
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			
			--Get the message which will be added as the notification text
			DECLARE @msgTxt VARCHAR(255)
			SELECT @msgTxt = p.Nickname + ' has left the game.' FROM tbl_Player p WHERE p.PlayerID = @PlayerID

			--Create the notifications for all other players in the game
			INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
			SELECT @msgTxt, 'LEAVE', 0, 1, @gameID, PlayerID
			FROM vw_ActiveAndVerifiedPlayers
			WHERE PlayerID <> @playerID AND GameID = @gameID
		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to create the join notification.'
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
-- Description:	Creates a tag notification for each player depending on the result of a tag.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID of the recipient does not exist
--		2. The gameID does not exist
--		3. When performing the update in the DB an error occurred

-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateTagResultNotification] 
	-- Add the parameters for the stored procedure here
	@takenByID INT,
	@photoOfID INT,
	@decision BIT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @msgTxt VARCHAR(255)
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_DATAINVALID INT = 17;

	BEGIN TRY  
		--Confirm the TakenByID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @takenByID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the PhotoOfID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @photoOfID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

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

		IF (@result = 1) --success
		BEGIN
			SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
			BEGIN TRANSACTION

				-- send to the tagged player
				SELECT @msgTxt = 'You have been tagged by ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @takenByID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @takenByGameID, @photoOfID) -- insert into table with specific playerID			
						
				-- send to the tagging player
				SELECT @msgTxt = 'You successfully tagged ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'SUCCESS', 0, 1, @takenByGameID, @takenByID) -- insert into table with specific playerID	
						
				-- send to everyone else						
				SELECT @msgTxt = p.Nickname + ' was tagged by ' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				SELECT @msgTxt += p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @takenByID

				--Create the notifications for all other players in the game
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
				SELECT @msgTxt, 'SUCCESS', 0, 1, @takenByGameID, PlayerID
				FROM vw_ActiveAndVerifiedPlayers
				WHERE PlayerID <> @takenByID AND PlayerID <> @photoOfID AND GameID = @takenByGameID						
			COMMIT
		END
		ELSE --fail
		BEGIN
		BEGIN
			SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
			BEGIN TRANSACTION

				-- send to the tagged player				
				SELECT @msgTxt = 'You were missed by ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @takenByID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @takenByGameID, @photoOfID) -- insert into table with specific playerID			
						
				-- send to the tagging player
				SELECT @msgTxt = 'You failed to tag ' + p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) VALUES (@msgTxt, 'FAIL', 0, 1, @takenByGameID, @takenByID) -- insert into table with specific playerID	
						
				-- send to everyone else		
				SELECT @msgTxt = p.Nickname + ' failed to tag ' FROM tbl_Player p WHERE p.PlayerID = @takenByID
				SELECT @msgTxt += p.Nickname + '.' FROM tbl_Player p WHERE p.PlayerID = @photoOfID
				
				--Create the notifications for all other players in the game
				INSERT INTO tbl_Notification(MessageText, NotificationType, IsRead, NotificationIsActive, GameID, PlayerID) 
				SELECT @msgTxt, 'FAIL', 0, 1, @takenByGameID, PlayerID
				FROM vw_ActiveAndVerifiedPlayers
				WHERE PlayerID <> @takenByID AND PlayerID <> @photoOfID AND GameID = @takenByGameID						
			COMMIT
		END
		END	
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the Notification table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to create the join notification.'
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
	@gameID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Confirm the playerID passed in exists and is active
	EXEC [dbo].[usp_ConfirmGameExistsAndIsActive] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	UPDATE tbl_Game
	SET GameIsActive = 0, GameIsDeleted = 1
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
	BEGIN TRY
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

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








USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dylan Levin
-- Create date: 13/09/18
-- Description:	Removes a player to a game.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record

-- =============================================
CREATE PROCEDURE [dbo].[usp_LeaveGame] 
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
	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY
		
		SET @result = 111;

		--Confirm the playerID passed in exists
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID)
		BEGIN
			SET @result = 111;
			SET @errorMSG = 'The playerID does not exist';
			RAISERROR('ERROR: playerID does not exist',16,1);
		END;

		-- fetch gameID from player
		DECLARE @gameID INT;
		SELECT @gameID = GameID FROM tbl_Player WHERE PlayerID = @playerID

		SET @result = 112;

		-- set player HasLeftGame to true
		UPDATE tbl_Player SET HasLeftGame = 1 WHERE PlayerID = @playerID

		SET @result = 113;

		-- decrement number of players in respective game
		UPDATE tbl_Game SET NumOfPlayers = NumOfPlayers-1 WHERE GameID = @gameID

		-- delete photos submitted by player that are not voted upon yet
		UPDATE tbl_Photo SET PhotoIsDeleted = 1 WHERE TakenByPlayerID = @playerID AND IsVotingComplete = 0
	
		-- delete votes submitted by player that have not reached an outcome
		UPDATE tbl_PlayerVotePhoto SET PlayerVotePhotoIsDeleted = 1 WHERE PlayerID = @playerID AND PlayerVotePhotoIsActive = 0
	
		-- delete any unread notifications for the player
		UPDATE tbl_Notification SET NotificationIsDeleted = 1 WHERE PlayerID = @playerID

		--Set the success return variables
		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to remove the player from the game'
		END
	END CATCH
END
GO






-- =============================================
-- Author:		Jonathan Williams
-- Create date: 18/09/18
-- Description:	Gets the player record matching the specified ID
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPlayerByID] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Confirm the GameID passed in exists and is active
	EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	SELECT *
	FROM vw_PlayerGame
	WHERE PlayerID = @playerID

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
-- Create date: 19/09/18
-- Description:	Updates the GameState of a game

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. @EC_INSERTERROR - An error occurred while trying to update the player record.

-- =============================================
CREATE PROCEDURE [dbo].[usp_SetGameState] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@gameState VARCHAR(255),
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_INSERTERROR INT = 2

	BEGIN TRY  
		--Confirm the GameID passed in exists and is active
		EXEC [dbo].[usp_ConfirmGameExistsAndIsActive] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Game
			SET GameState = @gameState
			WHERE GameID = @gameID
		COMMIT
	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to update the player record.'
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
	@imgURL VARCHAR(MAX), 
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
		SELECT @gameIDToJoin = GameID FROM tbl_Game WHERE GameCode = @gameCode AND GameIsActive = 1 AND GameIsDeleted = 0
		IF(@gameIDToJoin IS NULL)
		BEGIN
			SET @result = @EC_GAMEDOESNOTEXIST;
			SET @errorMSG = 'The game code does not exist';
			RAISERROR('',16,1);
		END

		--Confirm the game you are trying to join is not completed
		EXEC [dbo].[usp_ConfirmGameNotCompleted] @id = @gameIDToJoin, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Check to see if the player is okay to join the game, as the player may be attempting to join a game which has already begun
		DECLARE @canJoin BIT;
		SELECT @canJoin = IsJoinableAtAnytime FROM tbl_Game WHERE GameID = @gameIDToJoin
		DECLARE @gameState VARCHAR(255);
		SELECT @gameState = GameState FROM tbl_Game WHERE GameID = @gameIDToJoin

		--If the player cannot join at anytime and the current game state is currently PLAYING, return an error because the player
		--is trying to join a game which has already begun and does not allow players to join at anytime
		IF(@canJoin = 0 AND @gameState IN ('STARTING','PLAYING'))
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
				SET @errorMSG = 'The email address you entered is already taken by another player in an active/not complete game. Please enter a unique contact.';
				RAISERROR('',16,1);
			END
		END

		--If reaching this point all the inputs have been validated. Create the player record
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			IF(@isPhone = 1)
			BEGIN
				INSERT INTO tbl_Player(Nickname, Phone, SelfieDataURL, GameID, VerificationCode, IsHost) VALUES (@nickname, @contact, @imgURL, @gameIDToJoin, @verificationCode, @isHost);
			END
			ELSE
			BEGIN
				INSERT INTO tbl_Player(Nickname, Email, SelfieDataURL, GameID, VerificationCode, IsHost) VALUES (@nickname, @contact, @imgURL, @gameIDToJoin, @verificationCode, @isHost);
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
-- Create date: 18/09/18
-- Description:	Gets the player's ammo count
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetAmmoCount] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@ammoCount INT OUTPUT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @ammoCount = -1;

	--Confirm the PlayerID passed in exists and is active
	EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	SELECT @ammoCount = AmmoCount
	FROM tbl_Player
	WHERE PlayerID = @playerID

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
-- Description:	Begins the Game, sets the GameState to Starting and sets the StartTime and EndTime
--				Business Logic Code is scheduled to Run at start time to start the game and update the Game State to PLAYING.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. @EC_BEGINGAME_NOTHOST - The playerID is not the host of the game
--		3. @EC_BEGINGAME_NOTINLOBBY - The game is not currently IN LOBBY
--		4. @EC_BEGINGAME_NOTENOUGHPLAYERS - Trying to start a game with less than 3 active and verified players.

-- =============================================
CREATE PROCEDURE [dbo].[usp_BeginGame] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_BEGINGAME_NOTHOST INT = 6000;
	DECLARE @EC_BEGINGAME_NOTINLOBBY INT = 6001;
	DECLARE @EC_BEGINGAME_NOTENOUGHPLAYERS INT = 6002;

	BEGIN TRY  
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result
		

		--Confirm the playerID passed in Is the host of the Game because only the host can begin the game
		DECLARE @isHost BIT;
		SELECT @isHost = IsHost FROM tbl_Player WHERE PlayerID = @playerID
		IF(@isHost = 0)
		BEGIN
			SET @result = @EC_BEGINGAME_NOTHOST;
			SET @errorMSG = 'The playerID passed in is not the host player. Only the host can begin the game.';
			RAISERROR('',16,1);
		END

		--Get the GameID from the playerID
		DECLARE @gameID INT;
		EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT


		--Confirm the Game is IN LOBBY state
		DECLARE @gameState VARCHAR(255);
		SELECT @gameState = GameState FROM tbl_Game WHERE GameID = @gameID
		IF(@gameState NOT LIKE 'IN LOBBY')
		BEGIN
			SET @result = @EC_BEGINGAME_NOTINLOBBY;
			SET @errorMSG = 'The game state is not IN LOBBY, the must be in the lobby to begin the game.';
			RAISERROR('',16,1);
		END


		--Confirm there is enough players in the Game to Start
		DECLARE @activePlayerCount INT = 0;
		SELECT @activePlayerCount = COUNT(*) FROM tbl_Player WHERE GameID = @gameID AND IsVerified = 1 AND PlayerIsActive = 1 AND PlayerIsDeleted = 0 AND HasLeftGame = 0
		IF(@activePlayerCount < 3)
		BEGIN
			SET @result = @EC_BEGINGAME_NOTENOUGHPLAYERS;
			SET @errorMSG = 'Not enough players to start the game, need a minimum of 3.';
			RAISERROR('',16,1);
		END


		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			--Update the GameState to STARTING, update the StartTime to 10mins in the future and update the EndTime to the endtime
			UPDATE tbl_Game
			SET GameState = 'STARTING', StartTime = DATEADD(MINUTE, 10, GETDATE()), EndTime = DATEADD(DAY, 1, GETDATE())
			WHERE GameID = @gameID
		COMMIT

		SET @result = 1
		SET @errorMSG = '';
		SELECT * FROM tbl_Game WHERE GameID = @gameID
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

	--Confirm the playerID passed in exists and is active
	EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
	EXEC [dbo].[usp_DoRaiseError] @result = @result

	SELECT *
	FROM vw_PlayerVoteJoinTables
	WHERE PlayerID = @playerID AND IsPhotoSuccessful IS NULL AND PlayerVotePhotoIsActive = 1 AND PlayerVotePhotoIsDeleted = 0
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
CREATE PROCEDURE [dbo].[usp_UpdateVerificationCode] 
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
	DECLARE @EC_VERIFYPLAYER_ALREADYVERIFIED INT = 2002;
	

	BEGIN TRY
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the playerID is in a non verified state
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID AND IsVerified = 0)
		BEGIN
			SET @result = @EC_VERIFYPLAYER_ALREADYVERIFIED;
			SET @errorMSG = 'The playerID is already verified.';
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
		SELECT * FROM tbl_Player WHERE PlayerID = @playerID
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
	@connectionID VARCHAR(255),
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_ITEMALREADYEXISTS INT = 14;

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the new connectionID does not already exists
		IF EXISTS (SELECT * FROM tbl_Player WHERE ConnectionID = @connectionID)
		BEGIN
			SET @result = @EC_ITEMALREADYEXISTS;
			SET @errorMSG = 'The connectionID already exists.';
			RAISERROR('',16,1);
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
-- =============================================
-- Author:		Dylan Levin
-- Create date: 14/09/18
-- Description:	Marks player notifications as read.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record

-- =============================================

-- create type to store incoming data into a table
DROP TYPE IF EXISTS udtNotifs
GO
CREATE TYPE udt_Notifs AS TABLE
    ( 
        notificationID INT
    )
GO

CREATE PROCEDURE [dbo].[usp_SetNotificationsRead] 
	-- Add the parameters for the stored procedure here
	@udtNotifs AS dbo.udt_Notifs READONLY,
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

	BEGIN TRY

		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Update the notifications to IsRead = 1
		UPDATE tbl_Notification
		SET IsRead = 1
		WHERE PlayerID = @playerID AND NotificationID IN (
				SELECT notificationID
				FROM @udtNotifs
			)

		--Set the success return variables
		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to remove the player from the game'
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


USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 11/09/18
-- Description:	Updates a PlayerVotePhoto record with a players vote decision

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The PlayerVotePhoto record does not exist with the specified VoteID and PlayerID
--		2. The Photo record voting is already completed
--		3. The photo voting finish time has already passed

-- =============================================
CREATE PROCEDURE [dbo].[usp_VoteOnPhoto] 
	-- Add the parameters for the stored procedure here 
	@voteID INT,
	@playerID INT,
	@isPhotoSuccessful BIT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Declaring the possible error codes returned
	DECLARE @EC_INSERTERROR INT = 2;
	DECLARE @EC_VOTEPHOTO_VOTERECORDDOESNOTEXIST INT = 4000;
	DECLARE @EC_VOTEPHOTO_VOTEALREADYCOMPLETE INT = 4001;
	DECLARE @EC_VOTEPHOTO_VOTEFINISHTIMEPASSED INT = 4002;

	BEGIN TRY
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the vote record exists
		IF NOT EXISTS (SELECT * FROM tbl_PlayerVotePhoto WHERE PlayerID = @playerID AND VoteID = @voteID AND PlayerVotePhotoIsActive = 1 AND IsPhotoSuccessful IS NULL AND PlayerVotePhotoIsDeleted = 0)
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTERECORDDOESNOTEXIST;
			SET @errorMSG = 'The PlayerVotePhoto record does not exist.';
			RAISERROR('',16,1);
		END

		--Get the photoID from the PlayerVotePhoto record
		DECLARE @photoID INT;
		SELECT @photoID = PhotoID FROM tbl_PlayerVotePhoto WHERE VoteID = @voteID

		--Confirm the voting is not already complete on the photo record
		DECLARE @isVotingCompleted BIT;
		SELECT @isVotingCompleted = IsVotingComplete FROM tbl_Photo WHERE PhotoID = @photoID
		IF(@isVotingCompleted = 1)
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTEALREADYCOMPLETE;
			SET @errorMSG = 'The PlayerVotePhoto record has already been completed.';
			RAISERROR('',16,1);
		END


		--Confirm the Voting Finish Time has not already passed
		DECLARE @votingFinishTime DATETIME2;
		SELECT @votingFinishTime = VotingFinishTime FROM tbl_Photo WHERE PhotoID = @photoID
		IF(@votingFinishTime < GETDATE())
		BEGIN
			SET @result = @EC_VOTEPHOTO_VOTEFINISHTIMEPASSED;
			SET @errorMSG = 'The voting finish time has already passed.';
			RAISERROR('',16,1);
		END



		--If reaching this point all pre-condition checks have passed successfully


		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			
			--Update the PlayerVotePhoto record with the decision
			UPDATE tbl_PlayerVotePhoto
			SET IsPhotoSuccessful = @isPhotoSuccessful
			WHERE VoteID = @voteID

			--Update the photo record with the number of Yes or No Votes
			DECLARE @countYes INT = 0;
			DECLARE @countNo INT = 0;
			SELECT @countYes = COUNT(*) FROM tbl_PlayerVotePhoto WHERE PhotoID = @photoID AND IsPhotoSuccessful = 1
			SELECT @countNo = COUNT(*) FROM tbl_PlayerVotePhoto WHERE PhotoID = @photoID AND IsPhotoSuccessful = 0
			UPDATE tbl_Photo
			SET NumYesVotes = @countYes, NumNoVotes = @countNo
			WHERE PhotoID = @photoID


			--Update the photo's IsVotingComplete field if all the player votes have successfully been completed
			IF NOT EXISTS (SELECT * FROM tbl_PlayerVotePhoto WHERE PhotoID = @photoID AND IsPhotoSuccessful IS NULL AND PlayerVotePhotoIsActive = 1 AND PlayerVotePhotoIsDeleted = 0)
			BEGIN
				UPDATE tbl_Photo
				SET IsVotingComplete = 1
				WHERE PhotoID = @photoID
			END
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_PlayerVoteJoinTables WHERE VoteID = @voteID
	END TRY

	BEGIN CATCH
		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to cast your vote.'
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
	DECLARE @EC_VERIFYPLAYER_ALREADYVERIFIED INT = 2002;
	

	BEGIN TRY
		
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the playerID is in a non verified state
		IF NOT EXISTS (SELECT * FROM tbl_Player WHERE PlayerID = @playerID AND IsVerified = 0)
		BEGIN
			SET @result = @EC_VERIFYPLAYER_ALREADYVERIFIED;
			SET @errorMSG = 'The playerID is already verified.';
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
			DECLARE @gameID INT;
			EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @playerID, @gameID = @gameID OUTPUT
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
-- Create date: 05/09/18
-- Description:	Completes a game and sets all references to the game to not active.

-- Returns: The result (1 = successful, anything else = error), and the error message associated with it

-- Possible Errors Returned:
--		1. EC_INSERTERROR - An error occurred while trying to insert the game record
--		2. EC_GAMENOTEXISTS - The GameID passed in dows not exist.

-- =============================================
CREATE PROCEDURE [dbo].[usp_CompleteGame] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @EC_INSERTERROR INT = 2;

	BEGIN TRY  
		
		--Confirm the gameID passed in exists and is active
		EXEC [dbo].[usp_ConfirmGameExistsAndIsActive] @id = @gameID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Update the Game to now be completed.
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			--Update the game to completed
			UPDATE tbl_Game
			SET GameIsActive = 0, GameState = 'COMPLETED'
			WHERE GameID = @gameID

			--Update all the players in the game to not active
			UPDATE tbl_Player
			SET PlayerIsActive = 0
			WHERE GameID = @gameID

			--Update all photos in the game to not active
			UPDATE tbl_Photo
			SET PhotoIsActive = 0
			WHERE GameID = @gameID

			--Delete any photos which voting has not yet been completed
			UPDATE tbl_Photo
			SET PhotoIsDeleted = 1
			WHERE IsVotingComplete = 0 AND GameID = @gameID

			--Delete any votes which have not yet been completed in the game
			UPDATE tbl_PlayerVotePhoto
			SET PlayerVotePhotoIsDeleted = 1
			WHERE PhotoID IN (SELECT PhotoID FROM tbl_Photo WHERE PhotoIsDeleted = 1 AND GameID = @gameID)

			--Update all votes to not active
			UPDATE tbl_PlayerVotePhoto
			SET PlayerVotePhotoIsActive = 0
			WHERE PlayerID IN (SELECT PlayerID FROM tbl_Player WHERE GameID = @gameID)

			--Update all notifications to not active
			UPDATE tbl_Notification
			SET NotificationIsActive = 0
			WHERE GameID = @gameID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';

	END TRY

	--An error occurred in the data validation
	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'The an error occurred while trying to complete the game.';
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
-- Create date: 18/09/18
-- Description:	Gets all the players in a game with multiple filter parameters

--FILTER
--ALL = get all the players in the game which arnt deleted
--ACTIVE = get all players in the game which arnt deleted and is active
--INGAME = get all players in the game which arnt deleted, is active, have not left the game and have been verified
--INGAMEALL = get all players in the game which arnt deleted, is active, and have been verified (includes players who have left the game)

--ORDER by
--AZ = Order by name in alphabetical order
--ZA = Order by name in reverse alphabetical order
--KILLS= Order from highest to lowest in number of kills

-- Returns: 1 = Successful, or 0 = An error occurred
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetAllPlayersInGame] 
	-- Add the parameters for the stored procedure here
	@id INT,
	@isPlayerID BIT,
	@filter VARCHAR(255),
	@orderBy VARCHAR(255),
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	BEGIN TRY
		
		--If the id is a playerID get the GameID from the Player record
		IF(@isPlayerID = 1)
		BEGIN
			--Confirm the playerID exists
			EXEC [dbo].[usp_ConfirmPlayerExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
			EXEC [dbo].[usp_DoRaiseError] @result = @result

			--Get the GameID from the playerID
			DECLARE @gameID INT;
			EXEC [dbo].[usp_GetGameIDFromPlayer] @id = @id, @gameID = @gameID OUTPUT

			--Set the @id to the GameID so it can now be used in the query
			SET @id = @gameID
		END

		--Otherwise, confirm the GameID exists
		ELSE
		BEGIN
			EXEC [dbo].[usp_ConfirmGameExists] @id = @id, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
			EXEC [dbo].[usp_DoRaiseError] @result = @result
		END


		--If filter = ALL get all the players in the game which arnt deleted
		IF(@filter LIKE 'ALL')
		BEGIN
			SELECT * 
			FROM 
				tbl_Player 
			WHERE 
				PlayerIsDeleted = 0 AND
				GameID = @id
			ORDER BY
				CASE WHEN @orderBy LIKE 'AZ' THEN Nickname END ASC,
				CASE WHEN @orderBy LIKE 'ZA' THEN Nickname END DESC,
				CASE WHEN @orderBy LIKE 'KILLS' THEN NumKills END DESC
		END

		--If filer = ACTIVE get all players in the game which arnt deleted and active
		IF(@filter LIKE 'ACTIVE')
		BEGIN
			SELECT * 
			FROM 
				tbl_Player 
			WHERE 
				PlayerIsDeleted = 0 AND 
				PlayerIsActive = 1 AND
				GameID = @id
			ORDER BY
				CASE WHEN @orderBy LIKE 'AZ' THEN Nickname END ASC,
				CASE WHEN @orderBy LIKE 'ZA' THEN Nickname END DESC,
				CASE WHEN @orderBy LIKE 'KILLS' THEN NumKills END DESC
		END

		--If filer = INGAME get all players in the game which arnt deleted, isactive, have not left the game and have been verified
		IF(@filter LIKE 'INGAME')
		BEGIN
			SELECT * 
			FROM 
				tbl_Player 
			WHERE 
				PlayerIsDeleted = 0 AND 
				PlayerIsActive = 1 AND 
				HasLeftGame = 0 AND 
				IsVerified = 1 AND
				GameID = @id
			ORDER BY
				CASE WHEN @orderBy LIKE 'AZ' THEN Nickname END ASC,
				CASE WHEN @orderBy LIKE 'ZA' THEN Nickname END DESC,
				CASE WHEN @orderBy LIKE 'KILLS' THEN NumKills END DESC
		END

		--If filer = INGAMEALL get all players in the game which arnt deleted, isactive, and have been verified (includes players who have left the game)
		IF(@filter LIKE 'INGAMEALL')
		BEGIN
			SELECT * 
			FROM 
				tbl_Player 
			WHERE 
				PlayerIsDeleted = 0 AND 
				PlayerIsActive = 1 AND 
				IsVerified = 1 AND
				GameID = @id
			ORDER BY
				CASE WHEN @orderBy LIKE 'AZ' THEN Nickname END ASC,
				CASE WHEN @orderBy LIKE 'ZA' THEN Nickname END DESC,
				CASE WHEN @orderBy LIKE 'KILLS' THEN NumKills END DESC
		END

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
-- Author:		Jonathan Williams
-- Create date: 30/08/18
-- Description:	Decrements a players ammo count. When the player has taken a photo
--				their ammo is decremented and their number of photos taken is increased.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. @EC_DATAINVALID - The player's ammo is already at 0, cannot decrease ammo
--		2. @EC_INSERTERROR - An error occurred while trying to update the player record.

-- =============================================
CREATE PROCEDURE [dbo].[usp_UseAmmo] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_DATAINVALID INT = 17;
	DECLARE @EC_INSERTERROR INT = 2

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Confirm the Ammo Count is not already at 0
		DECLARE @ammoCount INT;
		SELECT @ammoCount = AmmoCount FROM tbl_Player WHERE PlayerID = @playerID
		IF(@ammoCount = 0)
		BEGIN
			SET @result = @EC_DATAINVALID;
			SET @errorMSG = 'The players ammo count is already at 0, cannot use ammo.';
			RAISERROR('', 16, 1);
		END

		--Make the update on the Players ammo count and number of photos taken
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET AmmoCount = AmmoCount - 1, NumPhotosTaken = NumPhotosTaken + 1
			WHERE PlayerID = @playerID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @playerID
	END TRY


	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to update the player record.'
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
-- Description:	Replenishes a players ammo by one.

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. @EC_INSERTERROR - An error occurred while trying to update the player record.

-- =============================================
CREATE PROCEDURE [dbo].[usp_ReplenishAmmo] 
	-- Add the parameters for the stored procedure here
	@playerID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EC_INSERTERROR INT = 2

	BEGIN TRY  
		--Confirm the playerID passed in exists and is active
		EXEC [dbo].[usp_ConfirmPlayerExistsAndIsActive] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
		EXEC [dbo].[usp_DoRaiseError] @result = @result

		--Make the update on the Players ammo count
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
		BEGIN TRANSACTION
			UPDATE tbl_Player
			SET AmmoCount = AmmoCount + 1
			WHERE PlayerID = @playerID
		COMMIT

		SET @result = 1;
		SET @errorMSG = '';
		SELECT * FROM vw_ActiveAndNotCompleteGamesAndPlayers WHERE PlayerID = @playerID
	END TRY


	--An error occurred in the data validation
	BEGIN CATCH
		--An error occurred while trying to perform the update on the PLayer table
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK;
			SET @result = @EC_INSERTERROR;
			SET @errorMSG = 'An error occurred while trying to update the player record.'
		END
	END CATCH
END
GO







--Dummy Data
INSERT INTO tbl_Game (GameCode, NumOfPlayers, GameState) VALUES ('tcf124', 4, 'IN LOBBY')
GO

INSERT INTO tbl_Player (Nickname, Email, SelfieDataURL, GameID, PlayerIsDeleted, IsVerified, IsHost) VALUES ('Jono', 'team6.camtag@gmail.com', 'localhost', 100000, 0, 1, 1)
GO
INSERT INTO tbl_Player (Nickname, Email, SelfieDataURL, GameID, PlayerIsDeleted, IsVerified) VALUES ('Dylan', 'team6.camtag@gmail.com', 'localhost', 100000, 0 ,1)
GO
INSERT INTO tbl_Player (Nickname, Email, SelfieDataURL, GameID, PlayerIsDeleted, IsVerified) VALUES ('Mathew', 'team6.camtag@gmail.com', 'localhost', 100000, 0, 1)
GO
INSERT INTO tbl_Player (Nickname, Email, SelfieDataURL, GameID, PlayerIsDeleted, IsVerified) VALUES ('Harry', 'team6.camtag@gmail.com', 'localhost', 100000, 0, 0)
GO
INSERT INTO tbl_Player (Nickname, Email, SelfieDataURL, GameID, PlayerIsDeleted, IsVerified) VALUES ('David', 'team6.camtag@gmail.com', 'localhost', 100000, 0, 1)
GO
INSERT INTO tbl_Player (Nickname, Email, SelfieDataURL, GameID, PlayerIsDeleted, IsVerified) VALUES ('Sheridan', 'team6.camtag@gmail.com', 'localhost', 100000, 0, 0)
GO
INSERT INTO tbl_Notification (MessageText, NotificationType, IsRead, NotificationIsActive, NotificationIsDeleted, GameID, PlayerID) VALUES ('ScoMo has joined the game.', 'JOIN', 0, 1, 0, 100000, 100000)
GO
INSERT INTO tbl_Notification (MessageText, NotificationType, IsRead, NotificationIsActive, NotificationIsDeleted, GameID, PlayerID) VALUES ('Somebody has left the game.', 'LEAVE', 1, 1, 0, 100000, 100000)
GO
INSERT INTO tbl_Notification (MessageText, NotificationType, IsRead, NotificationIsActive, NotificationIsDeleted, GameID, PlayerID) VALUES ('test IsRead 1', 'FAIL', 0, 1, 0, 100000, 100000)
GO
INSERT INTO tbl_Notification (MessageText, NotificationType, IsRead, NotificationIsActive, NotificationIsDeleted, GameID, PlayerID) VALUES ('test IsRead 2', 'FAIL', 0, 1, 0, 100000, 100000)
GO
INSERT INTO tbl_Photo (Lat, Long, PhotoDataURL, IsVotingComplete, PhotoIsDeleted, GameID, TakenByPlayerID, PhotoOfPlayerID) VALUES (-24.2, 130.0, 'localhost', 1, 0, 100000, 100001, 100002)