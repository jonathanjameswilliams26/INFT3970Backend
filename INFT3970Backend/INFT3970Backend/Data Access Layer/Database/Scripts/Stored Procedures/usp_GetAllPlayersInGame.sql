USE [udb_CamTag]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jonathan Williams
-- Create date: 1/09/18
-- Description:	Gets all the players in a game, takes in the game ID and gets all the players
--				in the game include inactive, deleted etc

-- Returns: 1 = Successful, or 0 = An error occurred
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetAllPlayersInGame] 
	-- Add the parameters for the stored procedure here
	@gameID INT,
	@result INT OUTPUT,
	@errorMSG VARCHAR(255) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRY
		
		SELECT *
		FROM vw_PlayerGame
		WHERE GameID = @gameID

		--Set the return variables
		SET @result = 1;
		SET @errorMSG = ''

	END TRY

	BEGIN CATCH

	END CATCH

END
GO