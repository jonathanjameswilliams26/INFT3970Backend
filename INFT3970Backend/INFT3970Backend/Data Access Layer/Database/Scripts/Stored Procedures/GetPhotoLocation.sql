-- =============================================
-- Author:		Jonathan Williams
-- Create date: 1/09/18
-- Description:	Gets all the players in a game, takes in a playerID and users that playerID to find all other players in the game

-- Returns: 1 = Successful, or 0 = An error occurred

-- Possible Errors Returned:
--		1. The playerID trying to update does not exist

-- =============================================
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
		WHERE GameID = @gameID AND IsActive = 1 AND IsVotingComplete = 1

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''
		
		PRINT 'Hello';

	END TRY

	BEGIN CATCH

	END CATCH

END
GO

DECLARE @errorID INT;
DECLARE @errorMessage VARCHAR(255);

EXEC usp_GetPlayerPhotoLocation 1000002, @errorID out, @errorMessage out 


--Dummy data
INSERT INTO tbl_Photo (Lat, Long, FilePath, IsVotingComplete, GameID, TakenByPlayerID, PhotoOfPlayerID) VALUES (-24.2, 130.0, 'localhost', 1, 100000, 100001, 100002);

