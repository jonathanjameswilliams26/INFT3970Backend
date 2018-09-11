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