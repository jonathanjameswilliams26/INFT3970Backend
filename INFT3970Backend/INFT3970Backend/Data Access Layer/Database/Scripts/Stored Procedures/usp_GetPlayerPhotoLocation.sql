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