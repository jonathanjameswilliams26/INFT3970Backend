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
		EXEC [dbo].[usp_ConfirmPlayerExists] @id = @playerID, @result = @result OUTPUT, @errorMSG = @errorMSG OUTPUT
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
			SET ConnectionID = @connectionID
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