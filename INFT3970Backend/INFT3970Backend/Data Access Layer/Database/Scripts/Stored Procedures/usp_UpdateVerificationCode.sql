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