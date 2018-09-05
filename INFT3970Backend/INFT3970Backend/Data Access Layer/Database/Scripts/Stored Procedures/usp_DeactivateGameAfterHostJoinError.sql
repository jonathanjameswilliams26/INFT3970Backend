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
	SET IsActive = 0
	WHERE GameID = @gameID

END
GO